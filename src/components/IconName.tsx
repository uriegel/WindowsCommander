import { IconNameType } from "../controllers/controller"
import "./IconName.css"

interface IconNameProps {
    namePart: string
    iconPath?: string
    type: IconNameType
}

const IconName = ({ namePart, type, iconPath }: IconNameProps) => 
    (<span> { type == IconNameType.Folder
        ? (<img className="image" src={`http://localhost:20000/iconfromname/folder`} alt="" />)
        : type == IconNameType.File
        ? (<img className="iconImage" src={`http://localhost:20000/iconfromextension/${iconPath}`} alt="" />)
        : type == IconNameType.Root && namePart == "C:\\"
        ? (<img className="image" src={`http://localhost:20000/iconfromname/windowsdrive`} alt="" />)
        : type == IconNameType.Root
        ? (<img className="image" src={`http://localhost:20000/iconfromname/drive`} alt="" />)
        : type == IconNameType.RootEjectable
        ? (<img className="image" src={`http://localhost:20000/iconfromname/media-removable`} alt="" />)
        : type == IconNameType.Home
        ? (<img className="image" src={`http://localhost:20000/iconfromname/user-home`} alt="" />)
        : type == IconNameType.Remote
        ? (<img className="image" src={`http://localhost:20000/iconfromname/network-server`} alt="" />)
        : type == IconNameType.Favorite
        ? (<img className="image" src={`http://localhost:20000/iconfromname/starred`} alt="" />)
        : (<img className="image" src={`http://localhost:20000/iconfromname/go-up`} alt="" />)
        }
        <span>{namePart}</span>
    </span>)

export default IconName