import type { TableColumns } from "virtual-table-react"
import { formatDateTime, formatSize, IconNameType, sortItems, type EnterData, type IController, type OnEnterResult } from "./controller"
import type { FileVersion, FolderViewItem } from "../components/FolderView"
import IconName from "../components/IconName"
import "../extensions/extensions"
import { onEnter, SelectedItemsType, type PrepareCopyResponse } from "../requests/requests"

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
        
        if (!enterData.item.isDirectory) {
            await onEnter({ id: enterData.id ?? "", name: enterData.item.name, path: enterData.path })
            return {
                processed: true
            }
        }
        else
            return {
                processed: false,
                pathToSet: enterData.item.isParent && (enterData.path.length == 3 || enterData.path.endsWith('$')) 
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
                