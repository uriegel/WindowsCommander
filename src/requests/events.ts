import { filter, map, share } from 'rxjs/operators'
import { webSocket } from "rxjs/webSocket"
import type { FolderViewItem } from '../components/FolderView'

export type CmdMsg = {
    cmd: string
}
export type CmdToggleMsg = {
    cmd: string,
    checked: boolean
}
export type StatusMsg = {
    folderId: string,
    requestId: number,
    text?: string
}
export type ExtendedInfo = {
    folderId: string,
    requestId: number,
    items: FolderViewItem[]
}

export type CopyProgress = {
    move: boolean
    name: string,
    totalCount: number,
    currentCount: number,
    totalMaxBytes: number,
    totalBytes: number,
    previousTotalBytes: number,
    currentMaxBytes: number,
    currentBytes: number,
    duration: number
}

type WebSocketMsg = {
    method: "cmd" | "cmdtoggle" | "status" | "extendedinfo" | "copyprogress",
    cmdMsg?: CmdMsg,
    cmdToggleMsg?: CmdToggleMsg,
    statusMsg?: StatusMsg
    extendedInfo?: ExtendedInfo,
    copyProgress?: CopyProgress,
    progressRunning?: boolean
}

const socket = webSocket<WebSocketMsg>('ws://localhost:20000/events').pipe(share())

export const cmdEvents = socket
                    .pipe(filter(n => n.method == "cmd"))
                    .pipe(map(n => n.cmdMsg)!)
export const cmdToggleEvents = socket
                    .pipe(filter(n => n.method == "cmdtoggle"))
                    .pipe(map(n => n.cmdToggleMsg!))
export const statusEvents = socket
                    .pipe(filter(n => n.method == "status"))
                    .pipe(map(n => n.statusMsg!))
export const exifDataEvents = socket
                    .pipe(filter(n => n.method == "extendedinfo"))
                    .pipe(map(n => n.extendedInfo!))
export const copyProgressEvents = socket
                    .pipe(filter(n => n.method == "copyprogress"))
                    .pipe(map(n => n.copyProgress!))

socket.subscribe({
    error: err => console.error('Subscription error:', err),
    complete: () => console.log('WebSocket connection closed'),
})