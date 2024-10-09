import React, {createContext, useContext, useMemo} from 'react';
import storage from "../utils/storage.ts";
import {AccountInfoResponse, auth, AuthUser, ExternalChallengeRequest, WhoAmIResponse} from "../api/auth.ts";
import {axios} from "../lib/axios.ts";
import Axios, {AxiosResponse, InternalAxiosRequestConfig} from 'axios';

const AuthContext = createContext<AuthContextProps>({user: null} as AuthContextProps);

export const AuthProvider = ({children}: { children: React.ReactNode }) => {
    const [user, setUser] = React.useState<AuthUser | null>(storage.getUser());
    
    axios.interceptors.request.use(authRequestInterceptor);
    axios.interceptors.response.use(value => value, async error => {
        if (!Axios.isAxiosError(error)) return Promise.reject(error);

        const originalConfig = error.config as any;
        const expired = error.response?.headers ? error.response.headers['x-token-expired'] : false;

        if (error.response?.status === 401 && expired && !originalConfig._retry) {
            originalConfig._retry = true;
            return tryRefreshToken(originalConfig);
        } else if (error.response?.status === 401 && !expired) {
            auth.signOut();
            setUser(null);
        }

        return Promise.reject(error);
    });

    const tryRefreshToken = async (originalRequestConfig: any) => {
        try {
            await auth.refreshToken();
            return axios.request(originalRequestConfig);
        } catch (error) {
            auth.signOut();
            setUser(null);

            return Promise.reject(error);
        }
    }

    const signIn = async (payload: SignInModel): Promise<void> => {
        return auth.signIn(payload).then((res) => {
            setUser(res);
        });
    };

    const challenge = async (data: ExternalChallengeRequest) => {
        const callbackUrl = data.mode == 'SignIn'
            ? window.location.origin + "/external-challenge-callback/" + data.provider
            : window.location.origin + "/profile?link=" + data.provider;
        auth.challengeExternal(data.provider, callbackUrl);
    };

    const signInExternal = async (): Promise<AuthUser> => {
        return auth.signInExternal().then((res) => {
            if ('email' in res) {
                setUser(res);
                return res;
            }

            throw Error("Invalid response");
        });
    };

    const linkLogin = async (): Promise<AccountInfoResponse> => {
        return auth.linkLogin();
    };

    const signUpExternal = async (payload: SignUpExternalModel): Promise<AuthUser | AxiosResponse> => {
        return auth.signUpExternal({
            ...payload,
        }).then((res) => {
            if ('email' in res) {
                setUser(res);
            }
            return res;
        });
    };

    const whoAmI = async (): Promise<WhoAmIResponse> => {
        return auth.whoAmI();
    };

    const signOut = () => {
        auth.signOut();
        setUser(null);
    };

    const value = useMemo<AuthContextProps>(
        () => ({
            user,
            signIn: signIn,
            signOut: signOut,
            signInExternal: signInExternal,
            signUpExternal: signUpExternal,
            whoAmI: whoAmI,
            challenge: challenge,
            linkLogin: linkLogin,
        }),
        [user]
    );

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
    return useContext(AuthContext);
};

export interface AuthContextProps {
    user: AuthUser | null;
    signIn: (payload: SignInModel) => Promise<void>;
    signOut: () => void;
    challenge: (data: ExternalChallengeRequest) => void;
    signInExternal: () => Promise<AuthUser>;
    linkLogin: () => Promise<AccountInfoResponse>;
    signUpExternal: (payload: SignUpExternalModel) => Promise<AuthUser | AxiosResponse>;
    whoAmI: () => Promise<WhoAmIResponse>;
}

export const authRequestInterceptor = (config: InternalAxiosRequestConfig) => {
    try {
        const token = storage.getToken();
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
    } catch (error) {
        console.log(error);
    }

    return config;
}

export interface SignUpExternalModel {
    // Optional fields, used for extended server example
    aboutMe?: string;
    mySiteUrl?: string;
}

export interface SignInModel {
    email: string;
    password: string;
    rememberMe: boolean;
}

export type ChallengeMode = 'SignIn' | 'LinkLogin';

