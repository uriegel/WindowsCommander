
import './Titlebar.css'
//import Pie from 'react-progress-control'
//import CopyProgress from "./dialogparts/CopyProgress"
import { DialogContext, ResultType } from "web-dialog-react"
import "functional-extensions"
import { useCallback, useContext, useEffect, useRef, useState, type JSX } from "react"

declare type WebViewType = {
    initializeCustomTitlebar: () => void,
    closeWindow(): () => void
}
declare const WebView: WebViewType

interface TitlebarProps {
    menu: JSX.Element
}

const Titlebar = ({ menu, }: TitlebarProps) => {
    
    const dialog = useContext(DialogContext)

    const [move, setMove] = useState(false)

    const dialogOpen = useRef(false)

    const [progressRevealed, setProgressRevealed] = useState(false)
	const [progressFinished, setProgressFinished] = useState(false)
	const [totalSize, setTotalSize] = useState(0)
    const [progress, setProgress] = useState(0)

    const startProgressDialog = useCallback(() => {
        const start = async () => {
            dialogOpen.current = true
            const res = await dialog.show({
                text: `Fortschritt beim ${move ? "Verschieben" : "Kopieren"} (${totalSize?.byteCountToString()})`,
                btnCancel: true,
                btnOk: true,
                btnOkText: "Stoppen",
//                extension: CopyProgress
            })
            dialogOpen.current = false
            // if (res?.result == ResultType.Ok)
            //     await webViewRequest("cancelcopy")
        }

        start()
    }, [dialog, move, totalSize])

    useEffect(() => {
        WebView.initializeCustomTitlebar()
    }, [])
    
	// useEffect(() => {
    //     // const startSubscription = startProgress.subscribe(e => {
    //     //     progressStartEvents.next(e)
	// 	// 	setProgressRevealed(true)
    //     //     setProgressFinished(false)
    //     //     setMove(e.isMove)
    // 	// 	setTotalSize(e.totalSize)
	// 	// })
    //     // const fileSubscription = fileProgress.subscribe(e => {
    //     //     progressFileEvents.next(e)
    //     // })
    //     // const bytesSubscription = byteProgress.subscribe(e => {
    //     //     progressBytesEvents.next(e)
    //     //     setProgress((e.currentBytes + e.completeCurrentBytes) / e.completeTotalBytes)
    //     // })
    //     // const finishedProgressSubscription = finishedProgress.subscribe(() => {
	// 	// 	setProgressFinished(true)
	// 	// })
    //     // const disposedProgressSubscription = disposedProgress.subscribe(() => {
	// 	// 	setProgressRevealed(false)
	// 	// })
	// 	return () => {
    //         startSubscription.unsubscribe()
    //         fileSubscription.unsubscribe()
    //         bytesSubscription.unsubscribe()
	// 		finishedProgressSubscription.unsubscribe()
	// 		disposedProgressSubscription.unsubscribe()
	// 	}
	// }, [])

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
            {/* <div className={`pieContainer${progressRevealed ? " revealed" : ""}${progressFinished ? " finished" : ""}`} onClick={startProgressDialog}>
                <Pie progress={progress}/>
            </div>             */}
            <div className="titlebarButton" id="$MINIMIZE$"><span className="dash">&#x2012;</span></div>
            <div className="titlebarButton" id="$RESTORE$"><span>&#10697;</span></div>  
            <div className="titlebarButton" id="$MAXIMIZE$"><span>&#9744;</span></div>
            <div className={"titlebarButton close"} id="$CLOSE$"><span>&#10005;</span></div>
            
            </div>)
}

export default Titlebar