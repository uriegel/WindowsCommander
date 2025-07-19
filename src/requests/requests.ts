import type { FolderViewItem } from "../components/FolderView"

interface ChangePath {
    id: string,
    path?: string
    mount?: boolean
    showHidden?: boolean
}

interface ChangePathResponse {
    cancelled?: boolean
    id: number
    controller?: string,
    dirCount: number,
    fileCount: number,
    items: FolderViewItem[],
    path?: string
}

interface PrepareCopy {
    id: string,
    path: string,
    targetPath: string,
    move: boolean,
    items: FolderViewItem[] 
}

export interface CopyItem {
    source: FolderViewItem,
    target: FolderViewItem
}

export interface PrepareCopyResponse {
    selectedItemsType: SelectedItemsType,
    totalSize: number,
    conflicts: CopyItem[]
    alreadyRunning?: boolean
}

interface Copy {
    id: string
    cancelled?: boolean
    notOverwrite?: boolean
}

interface CopyResponse {
    cancelled?: boolean,
    accessDenied?: boolean
}

interface GetExtended { id: number, folderId: string }
interface GetExtendedResponse { 
    cancelled?: boolean
}

interface SendMenuCmd {
    cmd: string
}

interface SendMenuCmdResponse {  
    cancelled?: boolean
}

interface OnEnter {
    id: string
    path: string,
    name: string,
    ctrl: boolean,
    alt: boolean
}

interface OnEnterResponse {
    success: boolean
}

interface Nil { nil?: boolean }

interface NilResponse { nil?: boolean }

export const SelectedItemsType = {
    None: 0,
    Folder: 1,
    Folders: 2,
    File: 3,
    Files: 4,
    Both: 5
}
export type SelectedItemsType = (typeof SelectedItemsType)[keyof typeof SelectedItemsType]

export const changePath = getJsonPost<ChangePath, ChangePathResponse>("changepath")
export const prepareCopy = getJsonPost<PrepareCopy, PrepareCopyResponse>("preparecopy")
export const copy = getJsonPost<Copy, CopyResponse>("copy")
export const getExtended = getJsonPost<GetExtended, GetExtendedResponse>("getextended")
export const sendMenuCmd = getJsonPost<SendMenuCmd, SendMenuCmdResponse>("sendmenucmd")
export const onEnter = getJsonPost<OnEnter, OnEnterResponse>("onenter")
export const cancelCopy = getJsonPost<Nil, NilResponse>("cancelcopy")
export const startUac = getJsonPost<Nil, NilResponse>("startuac")
export const stoptUac = getJsonPost<Nil, NilResponse>("stopuac", 21000)
export const copyUac = getJsonPost<Copy, CopyResponse>("copy", 21000)

function getJsonPost<RequestType, ResponseType>(method: string, port = 20000): (request: RequestType) => Promise<ResponseType> {
 
    async function jsonPost<RequestType, ResponseType>(method: string, request: RequestType): Promise<ResponseType> {
        const msg = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(request)
        }

        const response = await fetch(`http://localhost:${port}/request/${method}`, msg)
        const json = await response.text()
        return JSON.parse(json) as ResponseType
    }

    return (request: RequestType) => jsonPost(method, request)
}