import {Link} from "react-router-dom";

export const ResetPasswordConfirmation = () => {
    return (<>
        <h1>Your password has been reset.</h1>
        <p><strong><Link to={"/signin"}>Sign In </Link></strong></p>
    </>);
};