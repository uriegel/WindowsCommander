import type { TableColumns } from "virtual-table-react"
import { formatDateTime, formatSize, getItemsType, IconNameType, ItemsType, sortItems, type EnterData, type IController, type OnEnterResult } from "./controller"
import type { FileVersion, FolderViewItem } from "../components/FolderView"
import IconName from "../components/IconName"
import "../extensions/extensions"
import { createFolderRequest, deleteItems, deleteItemsUac, onEnter, renameItem, SelectedItemsType, setControllerUac, startUac, stoptUac, type PrepareCopyResponse } from "../requests/requests"
import { ResultType, type DialogHandle } from "web-dialog-react"
import { delayAsync } from "functional-extensions"

export class Directory implements IController {
    id: string

    getColumns(): TableColumns<FolderViewItem> {
        return {
            columns: [
    	        { name: "Name", isSortable: true, subColumn: "Erw." },
		        { name: "Datum", isSortable: true },
                { name: "Größe", isSortable: true, isRightAligned: true },
                { name: "Version", isSortable: true }
            ],
            getRowClasses,
            renderRow
        }
    }

    appendPath(path: string, subPath: string)  {
        return path.appendPath(subPath)
    } 

    async onEnter(enterData: EnterData): Promise<OnEnterResult> {
        
        if (!enterData.item.isDirectory || enterData.alt || enterData.ctrl) {
            await onEnter({ id: enterData.id ?? "", name: enterData.item.name, path: enterData.path, alt: enterData.alt, ctrl: enterData.ctrl })
            return {
                processed: true
            }
        }
        else
            return {
                processed: false,
                pathToSet: enterData.item.isParent && (enterData.path.length == 3) 
                    ? "root" 
                    : this.appendPath(enterData.path, enterData.item.name),
                latestPath: enterData.item.isParent ? enterData.path.extractSubPath() : undefined 
            }
    }

    getItems(): FolderViewItem[] { throw "not implemented" }

    sort(items: FolderViewItem[], sortIndex: number, sortDescending: boolean) {
        return sortItems(items, this.getSortFunction(sortIndex, sortDescending))     
    }

    getSortFunction = (index: number, descending: boolean) => {
        const ascDesc = (sortResult: number) => descending ? -sortResult : sortResult
        const sf = index == 0
            ? (a: FolderViewItem, b: FolderViewItem) => a.name.localeCompare(b.name) 
            : index == 1
                ? (a: FolderViewItem, b: FolderViewItem) => {	
                    const aa = a.exifData?.dateTime ? a.exifData?.dateTime : a.time || ""
                    const bb = b.exifData?.dateTime ? b.exifData?.dateTime : b.time || ""
                    return aa.localeCompare(bb) 
                } 
            : index == 2
            ? (a: FolderViewItem, b: FolderViewItem) => (a.size || 0) - (b.size || 0)
            : index == 3
            ? sortVersion
            : index == 10
            ? (a: FolderViewItem, b: FolderViewItem) => a.name.getExtension().localeCompare(b.name.getExtension()) 
            : undefined
        
        return sf
            ? (a: FolderViewItem, b: FolderViewItem) => ascDesc(sf(a, b))
            : undefined
    }

    itemsSelectable: boolean

    onSelectionChanged() { }
    
    getCopyText(prepareCopy: PrepareCopyResponse, move: boolean) {
        const copyAction = `${move ? "verschieben" : " kopieren"} (${prepareCopy.totalSize.byteCountToString()})`
        return prepareCopy.selectedItemsType == SelectedItemsType.File
            ? `Möchtest Du die Datei ${copyAction}?`
            : prepareCopy.selectedItemsType == SelectedItemsType.Folder
            ? `Möchtest Du das Verzeichnis ${copyAction}?`
            : prepareCopy.selectedItemsType == SelectedItemsType.Files
            ? `Möchtest Du die Dateien ${copyAction}?`
            : prepareCopy.selectedItemsType == SelectedItemsType.Folders
            ? `Möchtest Du die Verzeichnisse ${copyAction}?`
            : `Möchtest Du die Einträge ${copyAction}?`
    }

    async deleteItems(items: FolderViewItem[], dialog: DialogHandle, id: string, path: string) {
        const type = getItemsType(items)
        const text = type == ItemsType.Directory
            ? "Möchtest Du das Verzeichnis löschen?"
            : type == ItemsType.Directories
            ? "Möchtest Du die Verzeichnisse löschen?"
            : type == ItemsType.File
            ? "Möchtest Du die Datei löschen?"
            : type == ItemsType.Files
            ? "Möchtest Du die Dateien löschen?"		
            : "Möchtest Du die Verzeichnisse und Dateien löschen?"	
        
        const res = await dialog.show({
            text,
            btnOk: true,
            btnCancel: true,
            defBtnOk: true
        })
        if (res.result == ResultType.Cancel)        
            return false

        await delayAsync(50)
        dialog.show({text: "Löschen..." })
        let response = await deleteItems({ id, path, items: items.map(n => n.name)})
        if (response.accessDenied) {
            if (!(await startUac({})).success) {
                dialog.close()
                return false
            }
            await setControllerUac({id, path })
            response = await deleteItemsUac({ id, path, items: items.map(n => n.name)})
            await stoptUac({})
        }
        dialog.close()
        return !response.error
    }

    async rename(dialog: DialogHandle, id: string, path: string, selected: FolderViewItem, asCopy: boolean) {
        const res = await dialog.show({
            text: asCopy 
                ? `Möchtest du eine Kopie anlegen?`
                : `Möchtest du ${selected.isDirectory ? "das Verzeichnis" : "die Datei"} umbenennen?`,
            inputText: selected.name,
            btnOk: true,
            btnCancel: true,
            defBtnOk: true
        })
        if (res.result != ResultType.Ok || !res.input || selected.name == res.input )        
            return false

        var result = await renameItem({id, path, asCopy, item: selected.name, newName: res.input })
        return !result.error 
    }

    async createFolder(dialog: DialogHandle, id: string, path: string, selected: FolderViewItem|null) {
        const res = await dialog.show({
            text: `Möchtest du einen neuen Ordner anlegen?`,
            inputText: selected?.name,
            btnOk: true,
            btnCancel: true,
            defBtnOk: true
        })
        if (res.result != ResultType.Ok || !res.input || selected?.name == res.input )        
            return false

        var result = await createFolderRequest({id, path, newName: res.input })
        return !result.error 
    }

    constructor() {
        this.id = "ROOT"
        this.itemsSelectable = true
    }
}

const getRowClasses = (item: FolderViewItem) => 
	item.isHidden
		? ["hidden"]
		: []

const renderRow = (item: FolderViewItem) => [
	(<IconName namePart={item.name} type={
			item.isParent
			? IconNameType.Parent
			: item.isDirectory
			? IconNameType.Folder
			: IconNameType.File}
		iconPath={item.icon} />),
	(<span className={item.exifData?.dateTime ? "exif" : "" } >{formatDateTime(item?.exifData?.dateTime ?? item?.time)}</span>),
    formatSize(item.size),
    formatVersion(item.fileVersion)
]

const formatVersion = (version?: FileVersion) => 
    version ? `${version.major}.${version.minor}.${version.build}.${version.patch}` : ""

const sortVersion = (a: FolderViewItem, b: FolderViewItem) =>
    a.fileVersion && !b.fileVersion
    ? 1
    : !a.fileVersion && b.fileVersion   
    ? -1
    : a.fileVersion && b.fileVersion   
    ? a.fileVersion.major > b.fileVersion.major
    ? 1
    : a.fileVersion.major < b.fileVersion.major
    ? -1
    : a.fileVersion.minor > b.fileVersion.minor
    ? 1                
    : a.fileVersion.minor < b.fileVersion.minor
    ? -1                
    : a.fileVersion.build > b.fileVersion.build
    ? 1                
    : a.fileVersion.build < b.fileVersion.build
    ? -1                
    : a.fileVersion.patch > b.fileVersion.patch
    ? 1                
    : a.fileVersion.patch < b.fileVersion.patch
    ? -1                
    : 0
    : 0            
                