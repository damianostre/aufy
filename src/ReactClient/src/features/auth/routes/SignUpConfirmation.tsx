import {Link, useLocation} from "react-router-dom";
import {SignUpResponse} from "../../../api/auth.ts";

export const SignUpConfirmation = () => {
    const location = useLocation();
    const { requiresEmailConfirmation } = location.state as SignUpResponse;

    return (<>
        <h1>Congratulations! You have successfully signed up!</h1>
        {requiresEmailConfirmation && <>
            <p>Please check your email for confirmation link and Sign In.</p>
            <p>Don't get the email? 
                <strong><Link to={"/resend-confirm-email"}> Resend confirmation email</Link></strong>
            </p>
        </>}
        <p><strong><Link to={"/signin"}>Sign In</Link></strong></p>
    </>);
};