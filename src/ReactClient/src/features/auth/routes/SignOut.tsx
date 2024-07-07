import {Navigate} from "react-router-dom";
import {useAuth} from "../../../providers/AuthProvider.tsx";

export const SignOut = () => {    
    const { signOut } = useAuth();    
    signOut();
    
    return <Navigate to={"/"}/>;
};