import {useAuth} from "../../../providers/AuthProvider.tsx";
import {useNavigate, useSearchParams} from "react-router-dom";
import {useEffect} from "react";

export const ChallengeCallback = () => {
    const navigate = useNavigate();
    const [params] = useSearchParams();
    const signup = params.get("signup");
    const failed = params.get("failed");

    const auth = useAuth();
    useEffect(() => {
        if (failed) {
            navigate("/signin")
        } else if (!signup) {
            auth.signInExternal().then(() => {
                navigate("/profile")
            }).catch(() => {
                navigate("/signin?error=external-signin-failed")
            });
        } else {
            navigate("/signup-external")
        }
    }, [signup, failed]);

    return (
        <h1>Authenticating...</h1>
    );
};
