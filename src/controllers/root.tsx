import type { TableColumns } from "virtual-table-react"
import { formatSize, IconNameType, type EnterData, type IController, type OnEnterResult } from "./controller"
import type { FolderViewItem } from "../components/FolderView"
import IconName from "../components/IconName"

export class Root implements IController {
    id: string

    getColumns(): TableColumns<FolderViewItem> {
        return {
            columns: [
                { name: "Name" },
                { name: "Bezeichnung" },
                { name: "Größe", isRightAligned: true }
            ],
            getRowClasses,
            renderRow
        }
    }

    appendPath(_: string, subPath: string) {
        return subPath
    } 

    async onEnter(enterData: EnterData): Promise<OnEnterResult> {
        return {
            processed: false,
            pathToSet: enterData.item.name,
        }
    }

    getItems(): FolderViewItem[] { throw "not implemented" }

    sort(items: FolderViewItem[]) { return items }

    itemsSelectable: boolean

    onSelectionChanged() { }
    
    getCopyText() { return "" }
    
    async deleteItems() {}

    constructor() {
        this.id = "ROOT"
        this.itemsSelectable = false
    }
}

const getRowClasses = (item: FolderViewItem) => 
    item.isMounted == false
        ? ["notMounted"]
        : []

const renderRow = (item: FolderViewItem) => [
    (<IconName namePart={item.name} type={item.name != "fav" ? IconNameType.Root : IconNameType.Favorite } />),
    item.description ?? "",
    formatSize(item.size)
]