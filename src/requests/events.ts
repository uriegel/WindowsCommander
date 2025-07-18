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
    // string Title,
    // string Name,
    totalCount: number,
    currentCount: number,
    totalMaxBytes: number,
    totalBytes: number,
    previousTotalBytes: number,
    currentMaxBytes: number,
    currentBytes: number,
    isRunning: boolean,
//    TimeSpan Duration
}

type WebSocketMsg = {
    method: "cmd" | "cmdtoggle" | "status" | "extendedinfo" | "progressrevealed" | "copyprogress" | "progressrunning",
    cmdMsg?: CmdMsg,
    cmdToggleMsg?: CmdToggleMsg,
    statusMsg?: StatusMsg
    extendedInfo?: ExtendedInfo,
    progressRevealed?: boolean,
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
export const progressRevealedEvents = socket
                    .pipe(filter(n => n.method == "progressrevealed"))
                    .pipe(map(n => n.progressRevealed!))
export const copyProgressEvents = socket
                    .pipe(filter(n => n.method == "copyprogress"))
                    .pipe(map(n => n.copyProgress!))
export const progressRunningEvents = socket
                    .pipe(filter(n => n.method == "progressrunning"))
                    .pipe(map(n => n.progressRunning!))

socket.subscribe({
    error: err => console.error('Subscription error:', err),
    complete: () => console.log('WebSocket connection closed'),
})