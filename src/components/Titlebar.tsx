
import './Titlebar.css'
import Pie from 'react-progress-control'
import { DialogContext, ResultType } from "web-dialog-react"
import "functional-extensions"
import { forwardRef, useCallback, useContext, useEffect, useImperativeHandle, useRef, useState, type JSX } from "react"
import type { CopyProgress } from '../requests/events'
import CopyProgressPart from './dialogparts/CopyProgress'
import { cancelCopy } from '../requests/requests'

declare type WebViewType = {
    initializeCustomTitlebar: () => void,
    closeWindow(): () => void
}
declare const WebView: WebViewType

interface TitlebarProps {
    menu: JSX.Element
    copyProgress: CopyProgress
    progressRevealed: boolean
    progressFinished: boolean
}

export type TitlebarHandle = {
    startProgressDialog: ()=>void
}

const Titlebar = forwardRef<TitlebarHandle, TitlebarProps>(({ menu, copyProgress, progressFinished, progressRevealed }, ref) => {
    
    useImperativeHandle(ref, () => ({
        startProgressDialog
    }))

    const dialog = useContext(DialogContext)

    const dialogOpen = useRef(false)

    const move = false

    const [progress, setProgress] = useState(0)

    const startProgressDialog = useCallback(() => {
        const start = async () => {
            dialogOpen.current = true
            const res = await dialog.show({
                text: `Fortschritt beim ${move ? "Verschieben" : "Kopieren"} (${copyProgress.totalMaxBytes.byteCountToString()})`,
                btnCancel: true,
                btnOk: true,
                btnOkText: "Stoppen",
                extension: CopyProgressPart
             })
            dialogOpen.current = false
            if (res?.result == ResultType.Ok)
                await cancelCopy({})
        }

        start()
    }, [dialog, move, copyProgress])

    useEffect(() => {
        setProgress((copyProgress.totalBytes + copyProgress.currentBytes) / copyProgress.totalMaxBytes)
    }, [copyProgress])

    useEffect(() => {
        WebView.initializeCustomTitlebar()
    }, [])
    
    useEffect(() => {
        if (dialogOpen.current)
            dialog.close()

    }, [progressRevealed, dialog])

    return (
        <div className="titlebar">
            <img alt="" src={`http://localhost:20000/windowicon`} />
            {menu}
            <div className="titlebarGrip" id="$DRAG_REGION$">
                <span id="$TITLE$"></span>
            </div>
            <div className={`pieContainer${progressRevealed ? " revealed" : ""}${progressFinished ? " finished" : ""}`} onClick={startProgressDialog}>
                <Pie progress={progress}/>
            </div>             
            <div className="titlebarButton" id="$MINIMIZE$"><span className="dash">&#x2012;</span></div>
            <div className="titlebarButton" id="$RESTORE$"><span>&#10697;</span></div>  
            <div className="titlebarButton" id="$MAXIMIZE$"><span>&#9744;</span></div>
            <div className={"titlebarButton close"} id="$CLOSE$"><span>&#10005;</span></div>
            
            </div>)
})

export default Titlebar