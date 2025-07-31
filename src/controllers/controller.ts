import type { TableColumns } from "virtual-table-react"
import { Root } from "./root"
import type { FolderViewItem } from "../components/FolderView"
import { Directory } from "./directory"
import type { PrepareCopyResponse } from "../requests/requests"
import { Favorites } from "./favorites"
import type { DialogHandle } from "web-dialog-react"

export const IconNameType = {
    Parent: 'Parent',
    Root: 'Root',
    RootEjectable: 'RootEjectable',
    Home: 'Home',
    Folder: 'Folder',
    File: 'File',
    Remote: 'Remote',
    Android: 'Android',
    New: 'New',
    Service: 'Service',
    Favorite: 'Favorite'
}
export type IconNameType = (typeof IconNameType)[keyof typeof IconNameType]

export const ItemsType = {
    Directories: 'Directories',
    Directory: 'Directory',
    Files: 'Files',
    File: 'File',
    All: 'All',
}
export type ItemsType = (typeof ItemsType)[keyof typeof ItemsType]

export interface OnEnterResult {
    processed: boolean
    pathToSet?: string
    latestPath?: string
    mount?: boolean
    refresh?: boolean
}

export interface EnterData {
    id?: string
    path: string,
    item: FolderViewItem, 
    selectedItems?: FolderViewItem[]
    dialog: DialogHandle
    otherPath?: string
    ctrl: boolean
    alt: boolean
}

export interface IController {
    id: string 
    getColumns(): TableColumns<FolderViewItem>
    appendPath(path: string, subPath: string): string
    getItems: () => FolderViewItem[]
    onEnter: (data: EnterData) => Promise<OnEnterResult> 
    sort: (items: FolderViewItem[], sortIndex: number, sortDescending: boolean) => FolderViewItem[]
    itemsSelectable: boolean
    onSelectionChanged: (items: FolderViewItem[]) => void 
    getCopyText: (prepareCopy: PrepareCopyResponse, move: boolean) => string
    deleteItems: (items: FolderViewItem[], dialog: DialogHandle, id: string, path: string) => Promise<boolean>
    rename: (item: FolderViewItem, dialog: DialogHandle, id: string, path: string, selected: FolderViewItem, asCopy: boolean) => Promise<boolean>
}

export function getController(id: string): IController {
    return id == "ROOT"
        ? new Root()
        : id == "FAV"
        ? new Favorites()
        : new Directory()
}

export const getViewerPath = (path: string) => 
    path.startsWith("remote")
    ? `http://${path.stringBetween("/", "/")}:8080/getfile/${path.substringAfter("/").substringAfter("/")}`
    : `http://localhost:20000/getfile/${path}`

export const formatSize = (num?: number) => {
    if (!num)
        return "0"
    if (num == -1)
        return ""
    let sizeStr = num.toString()
    const sep = '.'
    if (sizeStr.length > 3) {
        const sizePart = sizeStr
        sizeStr = ""
        for (let j = 3; j < sizePart.length; j += 3) {
            const extract = sizePart.slice(sizePart.length - j, sizePart.length - j + 3)
            sizeStr = sep + extract + sizeStr
        }
        const strfirst = sizePart.substring(0, (sizePart.length % 3 == 0) ? 3 : (sizePart.length % 3))
        sizeStr = strfirst + sizeStr
    }
    return sizeStr    
}

const dateFormat = Intl.DateTimeFormat("de-DE", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
})

const timeFormat = Intl.DateTimeFormat("de-DE", {
    hour: "2-digit",
    minute: "2-digit"
})

export function formatDateTime(dateStr?: string) {
    if (!dateStr || dateStr.startsWith("0001"))
        return ''
    const date = Date.parse(dateStr)
    return dateFormat.format(date) + " " + timeFormat.format(date)  
}

export type SortFunction = (a: FolderViewItem, b: FolderViewItem) => number

export const sortItems = (folderItemArray: FolderViewItem[], sortFunction?: SortFunction, sortDirs?: boolean) => {
    const unsortedDirs = folderItemArray.filter(n => n.isDirectory || n.isParent)
    const dirs = sortDirs ? unsortedDirs.sort((a, b) => a.name.localeCompare(b.name)) : unsortedDirs
    let files = folderItemArray.filter(n => !n.isDirectory) 
    files = sortFunction ? files.sort(sortFunction) : files
    return dirs.concat(files)
}

export const getItemsType = (items: FolderViewItem[]): ItemsType => {
	const dirs = items.filter(n => n.isDirectory)
	const files = items.filter(n => !n.isDirectory)
	return dirs.length == 0
		? files.length > 1
		? ItemsType.Files
		: ItemsType.File
		: dirs.length == 1
		? files.length != 0
		? ItemsType.All
		: ItemsType.Directory
		: dirs.length > 1
		? files.length != 0
		? ItemsType.All
		: ItemsType.Directories
		: ItemsType.All
}
