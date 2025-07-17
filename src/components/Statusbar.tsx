import { useEffect, useRef } from 'react'
import './Statusbar.css'

export interface StatusbarProps {
    path: string
    dirCount: number
    fileCount: number
    errorText: string | undefined
    setErrorText: (text: string | undefined) => void
    statusText: string | undefined
}

const Statusbar = ({ path, dirCount, fileCount, errorText, setErrorText, statusText  }: StatusbarProps) => {

    const timer = useRef(0)

    useEffect(() => {
        if (errorText) {
            clearTimeout(timer.current)
            timer.current = setTimeout(() => setErrorText(undefined), 5000)            
        }
    }, [errorText, setErrorText])

    const getClasses = () => ["statusbar", errorText
                                                ? "error"
                                                : statusText
                                                ? "status"
                                                : null]
                                                    .join(' ')
    return (
        <div className={getClasses()}>
            { errorText
                || (<>
                    <span>{statusText || path}</span>
                    <span className='fill'></span>
                    <span>{`${dirCount} Verz.`}</span>
                    <span className='lastStatus'>{`${fileCount} Dateien`}</span>
                </>)}
        </div>
    )
}

export default Statusbar