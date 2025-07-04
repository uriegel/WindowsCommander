import { forwardRef, useCallback, useEffect, useImperativeHandle, useRef, useState } from "react"
import './FolderView.css'
import VirtualTable, { type OnSort, type SelectableItem, type TableColumns, type VirtualTableHandle } from "virtual-table-react"
import { changePath as changePathRequest, getExtended, prepareCopy } from "../requests/requests"
import { getController, type IController } from "../controllers/controller"
import { Root } from "../controllers/root"
import { exifDataEvents, statusEvents } from "../requests/events"
import { filter } from "rxjs/operators"
import RestrictionView, { type RestrictionViewHandle } from "./RestrictionView"
import { initializeHistory } from "../history"

export type FolderViewHandle = {
    id: string
    setFocus: ()=>void
    processEnter: (item: FolderViewItem)=>Promise<void>
    refresh: (forceShowHidden?: boolean) => void
    getPath: () => string
    changePath: (path: string) => void
    insertSelection: () => void
    selectAll: () => void
    selectNone: () => void
    copyItems: (inactiveFolder: FolderViewHandle, move: boolean)=>Promise<void>
}

interface ItemCount {
    fileCount: number
    dirCount: number
}

interface FolderViewProp {
    id: string,
    showHidden: boolean
    onFocus: () => void
    onItemChanged: (path: string, isDir: boolean, latitude?: number, longitude?: number) => void
    onItemsChanged: (count: ItemCount)=>void
    onEnter: (item: FolderViewItem)=>void
    setStatusText: (text?: string)=>void
}

const DriveKind = {
    Unknown: 'Unknown',
    Ext4: 'Ext4',
    Ntfs: 'Ntfs',
    Vfat: 'Vfat',
    Home: 'Home',
}
type DriveKind = (typeof DriveKind)[keyof typeof DriveKind]

export interface FolderViewItem extends SelectableItem {
    name:         string
    size?:        number
    isParent?:    boolean
    isDirectory?: boolean
    icon?:        string
    // Root item
    description?: string
    mountPoint?:  string
    isMounted?: boolean
    isEjectable?: boolean
    driveKind?: DriveKind
    // FileSystem item
    time?:        string
    exifData?:    ExifData
    isHidden?:    boolean
    // Remotes item
    ipAddress?:   string
    isAndroid?:   boolean
    isNew?: boolean
    // ExtendedRename
    newName?:     string|null
    // Favorites
    path?: string | null
}

export type ExifData = {
    dateTime?: string 
    latitude?: number
    longitude?: number
}

const FolderView = forwardRef<FolderViewHandle, FolderViewProp>((
    { id, showHidden, onFocus, onItemChanged, onItemsChanged, onEnter, setStatusText },
    ref) => {
    
    const setItems = useCallback((items: FolderViewItem[], dirCount?: number, fileCount?: number) => {
        setStateItems(items)
        refItems.current = items
        if (dirCount != undefined || fileCount != undefined) {
            itemCount.current = { dirCount: dirCount || 0, fileCount: fileCount || 0 }
            onItemsChanged(itemCount.current)
        }
    }, [onItemsChanged])

    const getWidthsId = useCallback(() => `${id}-${controller.current.id}-widths`, [id])

    const setWidths = useCallback((columns: TableColumns<FolderViewItem>) => {
        const widthstr = localStorage.getItem(getWidthsId())
        const widths = widthstr ? JSON.parse(widthstr) as number[] : null
        return widths
            ? {
                ...columns, columns: columns.columns.map((n, i) => ({...n, width: widths![i]}))
            }
            : columns
    }, [getWidthsId])

    const changePath = useCallback(async (path?: string, forceShowHidden?: boolean, mount?: boolean, latestPath?: string, fromBacklog?: boolean, checkPosition?: (checkItem: FolderViewItem) => boolean) => {
        const result = await changePathRequest({ id, path, showHidden: forceShowHidden === undefined ? showHidden : forceShowHidden, mount })
        if (result.cancelled)
            return
        restrictionView.current?.reset()
        if (result.controller) {
            controller.current = getController(result.controller)
            virtualTable.current?.setColumns(setWidths(controller.current.getColumns()))
        }
        if (result.path)
            setPath(result.path)
        const newItems = controller.current.sort(result.items, sortIndex.current, sortDescending.current)
        setItems(newItems, result.dirCount, result.fileCount)
        getExtended({ id: result.id, folderId: id })
        const pos = latestPath
                    ? newItems.findIndex(n => n.name == latestPath)
                    : checkPosition
                    ? newItems.findIndex(n => checkPosition(n))
                    : 0        
        virtualTable.current?.setInitialPosition(pos, newItems.length)
        if (result.path) {
            localStorage.setItem(`${id}-lastPath`, result.path)
            if (!fromBacklog)
                history.current.set(result.path)
        }
    }, [id, setItems, setWidths, showHidden])

    useImperativeHandle(ref, () => ({
        id,
        setFocus() { virtualTable.current?.setFocus() },
        processEnter,
        refresh,
        getPath() { return path },
        changePath,
        insertSelection,
        selectAll,
        selectNone,
        copyItems
    }))

    const input = useRef<HTMLInputElement | null>(null)
    const restrictionView = useRef<RestrictionViewHandle>(null)

    const virtualTable = useRef<VirtualTableHandle<FolderViewItem>>(null)
    const itemCount = useRef({ fileCount: 0, dirCount: 0 })
    const sortIndex = useRef(0)
    const sortDescending = useRef(false)
    const history = useRef(initializeHistory())

    const [items, setStateItems] = useState([] as FolderViewItem[])
    const [path, setPath] = useState("")

    const controller = useRef<IController>(new Root())
    const refItems = useRef(items) 
    const changePathId = useRef(-1)

    const onPositionChanged = useCallback((item: FolderViewItem) => 
        onItemChanged(controller.current.appendPath(path, item.name),
            item.isDirectory == true, item.exifData?.latitude, item.exifData?.longitude),
    [path, onItemChanged])         

    useEffect(() => {
        changePath(localStorage.getItem(`${id}-lastPath`) ?? "root", false, false)
    }, [changePath, id]) 

    useEffect(() => {
        const subscription = statusEvents
            .pipe(filter(n => n.folderId == id))
            .subscribe(m => {
                if (changePathId.current <= m.requestId) {
                    setStatusText(m!.text)
                    changePathId.current = m.requestId
                }
            })
        return () => subscription.unsubscribe()
    }, [setStatusText, id])

    useEffect(() => {
        const subscription = exifDataEvents
            .pipe(filter(n => n.folderId == id))
            .subscribe(m => {
                if (changePathId.current <= m.requestId) {
                    const newItems = controller.current.sort(m!.items, sortIndex.current, sortDescending.current)
                    setItems(newItems)
                    changePathId.current = m.requestId
                }
            })
        return () => subscription.unsubscribe()
    }, [id, setItems])

    const processEnter = async (item: FolderViewItem) => {
        const res = await controller.current.onEnter({ path, item })
        if (!res.processed)
            changePath(res.pathToSet, showHidden, res.mount, res.latestPath)
    }

    const refresh = (forceShowHidden?: boolean) => changePath(path, forceShowHidden || (forceShowHidden === false ? false : showHidden))
    
    const onInputChange = (e: React.ChangeEvent<HTMLInputElement>) => setPath(e.target.value)

    const onInputKeyDown = (e: React.KeyboardEvent) => {
        if (e.code == "Enter") {
            changePath(path, showHidden)
            virtualTable.current?.setFocus()
            e.stopPropagation()
            e.preventDefault()
        }
    }

    const onInputFocus = (e: React.FocusEvent<HTMLInputElement>) => 
        setTimeout(() => e.target.select())

    const setSelection = (item: FolderViewItem, set: boolean) => {
        if (!item.isParent && !item.isNew)
            item.isSelected = set
        return item
    }

    const onKeyDown = async (evt: React.KeyboardEvent) => {
        switch (evt.code) {
            case "Home":
                if (evt.shiftKey && controller.current.itemsSelectable) 
                    setItems(items.map((n, i) => setSelection(n, i <= (virtualTable.current?.getPosition() ?? 0))))
                controller.current.onSelectionChanged(items)
                evt.preventDefault()
                evt.stopPropagation()
                break
            case "End":
                if (evt.shiftKey && controller.current.itemsSelectable) 
                    setItems(items.map((n, i) => setSelection(n, i >= (virtualTable.current?.getPosition() ?? 0))))
                controller.current.onSelectionChanged(items)                    
                evt.preventDefault()
                evt.stopPropagation()
                break
            case "Space": {
                const ri = restrictionView.current?.checkKey(" ")
                if (ri) {
                    virtualTable.current?.setPosition(0)
                    setItems(ri)
                } else if (controller.current.itemsSelectable)
                    setItems(items.map((n, i) => i != virtualTable.current?.getPosition() ? n : toggleSelection(n)))
                controller.current.onSelectionChanged(items)
                evt.preventDefault()
                evt.stopPropagation()
                break
            }
            case "Escape":
                if (!checkRestricted(evt.key)) {
                    if (controller.current.itemsSelectable) 
                        setItems(items.map((n) => setSelection(n, false)))
                    controller.current.onSelectionChanged(items)                    
                }
                break                
            // case "Delete":
            //     deleteItems()
            //     break
            case "Backspace":
                if (!checkRestricted(evt.key)) {
                    const path = history.current?.get(evt.shiftKey)
                    if (path)
                        changePath(path, showHidden, undefined, undefined, true)
                }
                break
            default:
                checkRestricted(evt.key)
                break
        }
    }

    const insertSelection = () => {
        if (controller.current.itemsSelectable) {
            setItems(items.map((n, i) => i != virtualTable.current?.getPosition() ? n : toggleSelection(n)))
            virtualTable.current?.setPosition(virtualTable.current.getPosition() + 1)
            controller.current.onSelectionChanged(items)
        }
    }

    const selectAll = () => {
        if (controller.current.itemsSelectable) {
            setItems(items.map((n) => setSelection(n, true)))
            controller.current.onSelectionChanged(items)
        }
    }

    const selectNone = () => {
        if (controller.current.itemsSelectable) {
            setItems(items.map((n) => setSelection(n, false)))
            controller.current.onSelectionChanged(items)
        }
    }

    const copyItems = async (inactiveFolder: FolderViewHandle, move: boolean) => {
        const prepareResult = prepareCopy({ id, move, path, targetPath: inactiveFolder.getPath(), items: getSelectedItems() })
        
        console.log(prepareResult)
    }

    const onSort = async (sort: OnSort) => {
        sortIndex.current = sort.isSubColumn ? 10 : sort.column
        sortDescending.current = sort.isDescending
        const newItems = controller.current.sort(items, sortIndex.current, sortDescending.current)
        setItems(newItems)
        const name = items[virtualTable.current?.getPosition() ?? 0].name
        virtualTable.current?.setPosition(newItems.findIndex(n => n.name == name))
    }

    const getSelectedItems = () => {

        const checkParent = (item: FolderViewItem) => !item.isParent ? item : null

        const selected = items.filter(n => n.isSelected)
        return selected.length > 0
            ? selected
            : [checkParent(items[virtualTable.current?.getPosition() ?? 0])].filter(n => n != null) as FolderViewItem[]
    }

    const checkRestricted = (key: string) => {
        const restrictedItems = restrictionView.current?.checkKey(key)
        if (restrictedItems) {
            virtualTable.current?.setPosition(0)
            setItems(restrictedItems)
            return true
        } else
            return false
    }

    const toggleSelection = (item: FolderViewItem) => {
        if (!item.isParent && !item.isNew)
            item.isSelected = !item.isSelected
        return item
    }

    const onColumnWidths = (widths: number[]) => {
        if (widths.length)
            localStorage.setItem(getWidthsId(), JSON.stringify(widths))
    }

    const onFocusChanged = useCallback(() => {
        onFocus()
        const pos = virtualTable.current?.getPosition() ?? 0
        const item = pos < items.length ? items[pos] : null 
        if (item)
            onPositionChanged(item)
        onItemsChanged(itemCount.current)
    }, [items, onFocus, onPositionChanged, onItemsChanged]) 

    return (
        <div className="folder" onFocus={onFocusChanged}>
            <input ref={input} className="pathInput" spellCheck={false} value={path} onChange={onInputChange} onKeyDown={onInputKeyDown} onFocus={onInputFocus} />
            <div className="tableContainer" onKeyDown={onKeyDown} >
                <VirtualTable ref={virtualTable} items={items} onColumnWidths={onColumnWidths} onEnter={onEnter} onPosition={onPositionChanged} onSort={onSort} />
            </div>
            <RestrictionView items={items} ref={restrictionView} />
        </div>
    )
})

export default FolderView