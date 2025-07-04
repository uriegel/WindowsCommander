import { useRef } from 'react'
import './themes/windows.css'
import Commander, { type CommanderHandle } from './components/Commander'
import './App.css'
import WithDialog from 'web-dialog-react'

const App = () => {

	const commander = useRef(null as CommanderHandle|null)

	const onKeyDown = (evt: React.KeyboardEvent) =>
		commander.current?.onKeyDown(evt)

	return (
		<div className="App adwaitaTheme" onKeyDown={onKeyDown}>
			<WithDialog>
				<Commander ref={commander} ></Commander>
			</WithDialog>
		</div>
	)
}

export default App