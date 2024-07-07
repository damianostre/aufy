import {Link, useSearchParams} from "react-router-dom";
import {auth} from "../../../api/auth.ts";
import {useEffect, useState} from "react";

export const ConfirmEmail = () => {
    const [params] = useSearchParams();
    const code = params.get("code");
    const userId = params.get("userId");
    const valid = code && userId;

    const [loading, setLoading] = useState(false);
    const [success, setSuccess] = useState(false);

    useEffect(() => {
            if (valid) {
                setLoading(true);
                auth.confirmEmail(code, userId).then(() => {
                    setLoading(false);
                    setSuccess(true);
                }).catch(() => {
                    setLoading(false);
                    setSuccess(false);
                });
            }
        },
        [valid, code, userId]);

    return (<>
        {!valid && <h1>Invalid request</h1>}
        {loading && <h1>Confirming email...</h1>}
        {!loading && !success && <>
            <h1>Something went wrong</h1>
            <p>Please try again later.</p>
        </>}
        {!loading && success && <>
            <h1>Email confirmed!</h1>
            <p><strong><Link to={"/signin"}>Sign In</Link></strong>
            </p>
        </>}
    </>);
};
