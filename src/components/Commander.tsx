import { forwardRef, useCallback, useContext, useEffect, useImperativeHandle, useRef, useState } from "react"
import ViewSplit from "view-split-react"
import type { FolderViewHandle, FolderViewItem } from "./FolderView"
import FolderView from "./FolderView"
import '../App.css'
import '../themes/windows.css'
import "functional-extensions"
import Statusbar from "./Statusbar"
import { cmdEvents, cmdToggleEvents, copyProgressEvents, progressRevealedEvents, progressRunningEvents, type CmdToggleMsg, type CopyProgress } from "../requests/events"
import Titlebar, { type TitlebarHandle } from "./Titlebar"
import Menu from "./Menu"
import PictureViewer from "./PictureViewer"
import MediaPlayer from "./MediaPlayer"
import FileViewer from "./FileViewer"
import { sendMenuCmd } from "../requests/requests"
import { DialogContext } from "web-dialog-react"
import type { SpecialKeys } from "virtual-table-react"

const ID_LEFT = "left"
const ID_RIGHT = "right"

const PreviewMode = {
    Default: 'Default',
    Location: 'Location',
    Both: 'Both'
}
type PreviewMode = (typeof PreviewMode)[keyof typeof PreviewMode]

interface ItemProperty {
	path: string
	latitude?: number 
	longitude?: number
	isDirectory: boolean
}

export type CommanderHandle = {
    onKeyDown: (evt: React.KeyboardEvent)=>void
}

const Commander = forwardRef<CommanderHandle, object>((_, ref) => {

    useImperativeHandle(ref, () => ({
        onKeyDown
    }))

	const folderLeft = useRef<FolderViewHandle>(null)
	const folderRight = useRef<FolderViewHandle>(null)

	const [showViewer, setShowViewer] = useState(false)    
	const [showHidden, setShowHidden] = useState(false)
	const [itemProperty, setItemProperty] = useState<ItemProperty>({ path: "", latitude: undefined, longitude: undefined, isDirectory: false })
	const [itemCount, setItemCount] = useState({ dirCount: 0, fileCount: 0 })
	const [statusTextLeft, setStatusTextLeft] = useState<string | undefined>(undefined)
	const [statusTextRight, setStatusTextRight] = useState<string | undefined>(undefined)
	const [errorText, setErrorText] = useState<string | undefined>(undefined)
	const [previewMode, setPreviewMode] = useState(PreviewMode.Default)
	const [progress, setProgress] = useState<CopyProgress>({
		move: false, name: "", duration: 0, currentBytes: 0, currentCount: 0, currentMaxBytes: 0, isRunning: false,
		previousTotalBytes: 0, totalBytes: 0, totalCount: 0, totalMaxBytes: 0
	})
	const [progressRevealed, setProgressRevealed] = useState(false)
	const [progressFinished, _setProgressFinished] = useState(false)
    
	const [activeFolderId, setActiveFolderId] = useState(ID_LEFT)
	const onFocusLeft = () => setActiveFolderId(ID_LEFT)
	const onFocusRight = () => setActiveFolderId(ID_RIGHT)

	const showHiddenRef = useRef(false)
	const showViewerRef = useRef(false)

	const dialog = useContext(DialogContext)

	const titlebar = useRef(null as TitlebarHandle|null)

	const onKeyDown = (evt: React.KeyboardEvent) => {
		if (evt.code == "Tab" && !evt.shiftKey) {
			getInactiveFolder()?.setFocus()
			evt.preventDefault()
			evt.stopPropagation()
		}
	}

	const getActiveFolder = useCallback(() => activeFolderId == ID_LEFT ? folderLeft.current : folderRight.current, [activeFolderId])
	const getInactiveFolder = useCallback(() => activeFolderId == ID_LEFT ? folderRight.current : folderLeft.current, [activeFolderId])

	const onMenuAction = useCallback(async (key: string) => {
		switch (key) {
			case "REFRESH":
				getActiveFolder()?.refresh()
				break
			case "ADAPT_PATH": {
				const path = getActiveFolder()?.getPath()
				if (path)
					getInactiveFolder()?.changePath(path)
				break
			}
			case "togglepreview":
				if (showViewer)
					setPreviewMode(previewMode == PreviewMode.Default
						? PreviewMode.Location
						: previewMode == PreviewMode.Location
						? PreviewMode.Both
						: PreviewMode.Default)			
				break
			case "PROPERTIES":
				getActiveFolder()?.showProperties()
				break
			case "OPENAS":
				getActiveFolder()?.openAs()
				break
			case "FAVORITES":
				getActiveFolder()?.changePath("fav")
				break
			case "insert":
				getActiveFolder()?.insertSelection()
				break
			case "SEL_ALL":
				getActiveFolder()?.selectAll()
				break
			case "SEL_NONE":
				getActiveFolder()?.selectNone()
				break
			case "COPY": {
					const other = getInactiveFolder()
					if (other)
						getActiveFolder()?.copyItems(other, false, getActiveFolder()?.id == ID_LEFT)
				}			
				break
			case "MOVE": {
					const other = getInactiveFolder()
					if (other)
						getActiveFolder()?.copyItems(other, true, getActiveFolder()?.id == ID_LEFT)
				}			
				break
			case "SHOW_DEV_TOOLS":
				await sendMenuCmd({ cmd: key })
				break
			case "END":
				window.close()
				break
		}
	}, [getActiveFolder, getInactiveFolder, previewMode, showViewer])

	const onMenuToggleAction = useCallback(async (msg: CmdToggleMsg) => {
		switch (msg.cmd) {
			case "showhidden": 
				setShowHidden(msg.checked)
				folderLeft.current?.refresh(msg.checked)
				folderRight.current?.refresh(msg.checked)
				break
			case "showpreview":
				setShowViewer(msg.checked)
				break
		}
	}, [])

	useEffect(() => {
		folderLeft.current?.setFocus()
	}, [])

	useEffect(() => {
		const subscription = cmdEvents.subscribe(m => onMenuAction(m!.cmd))
		const subscriptionToggle = cmdToggleEvents.subscribe(onMenuToggleAction)
		const subscriptionRevealed = progressRevealedEvents.subscribe(setProgressRevealed)
		const subscriptionProgress = copyProgressEvents.subscribe(setProgress)
		const subscriptionProgressRunning = progressRunningEvents.subscribe(titlebar.current?.startProgressDialog)
		return () => {
			subscriptionToggle.unsubscribe()
			subscription.unsubscribe()
			subscriptionRevealed.unsubscribe()
			subscriptionProgress.unsubscribe()
			subscriptionProgressRunning.unsubscribe()
		}
	}, [onMenuAction, onMenuToggleAction, titlebar])

	const toggleShowHiddenAndRefresh = () => {
		showHiddenRef.current = !showHiddenRef.current
		setShowHidden(showHiddenRef.current)
		folderLeft.current?.refresh(showHiddenRef.current)
		folderRight.current?.refresh(showHiddenRef.current)
	}
	
	const toggleShowViewer = () => {
		showViewerRef.current = !showViewerRef.current
		setShowViewer(showViewerRef.current)
	}

	const onItemChanged = useCallback(
		(path: string, isDirectory: boolean, latitude?: number, longitude?: number) => 
			setItemProperty({ path, isDirectory, latitude, longitude })
	, [])

	const onEnter = (item: FolderViewItem, specialKeys?: SpecialKeys) => 
		getActiveFolder()?.processEnter(item, getInactiveFolder()?.getPath(), specialKeys)
		

	const VerticalSplitView = () => (
		<ViewSplit firstView={FolderLeft} secondView={FolderRight}></ViewSplit>
    )
    
	const FolderLeft = () => (
		<FolderView ref={folderLeft} id={ID_LEFT} onFocus={onFocusLeft} onItemChanged={onItemChanged} onItemsChanged={setItemCount}
			onEnter={onEnter} showHidden={showHidden} setStatusText={setStatusTextLeft} setErrorText={setErrorText} dialog={dialog} />
	)
	const FolderRight = () => (
		<FolderView ref={folderRight} id={ID_RIGHT} onFocus={onFocusRight} onItemChanged={onItemChanged} onItemsChanged={setItemCount}
			onEnter={onEnter} showHidden={showHidden} setStatusText={setStatusTextRight} setErrorText={setErrorText} dialog={dialog} />
	)

	const getStatusText = useCallback(() => 
		activeFolderId == ID_LEFT ? statusTextLeft : statusTextRight
	, [activeFolderId, statusTextLeft, statusTextRight])

	const ViewerView = () => {
		const ext = itemProperty
					.path
					.getExtension()
					.toLocaleLowerCase()

		return ext == ".jpg" || ext == ".png" || ext == ".jpeg"
		 	? (<PictureViewer path={itemProperty.path} latitude={itemProperty.latitude} longitude={itemProperty.longitude} />)
		  	: ext == ".mp3" || ext == ".mp4" || ext == ".mkv" || ext == ".wav"
		  	? (<MediaPlayer path={itemProperty.path} />)
		  	: ext == ".pdf"
		  	? (<FileViewer path={itemProperty.path} />)
		  	: (<div></div>)
	}

	return (
		<>
			<Titlebar ref={titlebar} menu={(
				<Menu autoMode={false} onMenuAction={onMenuAction}
					showHidden={showHidden} toggleShowHidden={toggleShowHiddenAndRefresh}
					showViewer={showViewer} toggleShowViewer={toggleShowViewer} />
			)} copyProgress={progress} progressFinished={progressFinished} progressRevealed={progressRevealed} />			
			<ViewSplit isHorizontal={true} firstView={VerticalSplitView} secondView={ViewerView} initialWidth={30} secondVisible={showViewer} />
			<Statusbar path={itemProperty.path} dirCount={itemCount.dirCount} fileCount={itemCount.fileCount}
					errorText={errorText} setErrorText={setErrorText} statusText={getStatusText()} />		
		</>
	)
})

    export default Commander