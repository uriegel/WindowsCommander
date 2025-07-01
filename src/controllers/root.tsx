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

    sort(items: FolderViewItem[]) { return items }

    itemsSelectable: boolean

    onSelectionChanged() { }
    
    getCopyText() { return ""}

    constructor() {
        this.id = "ROOT"
        this.itemsSelectable = false
    }
}

const REMOTES = "remotes"
const FAVORITES = "fav"

const getRowClasses = (item: FolderViewItem) => 
    item.isMounted == false
        ? ["notMounted"]
        : []

const renderRow = (item: FolderViewItem) => [
    (<IconName namePart={item.name} type={IconNameType.Root} />),
    item.description ?? "",
    formatSize(item.size)
]