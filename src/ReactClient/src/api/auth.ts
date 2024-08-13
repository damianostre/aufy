import storage from "../utils/storage.ts";
import {axios, axiosCreate} from "../lib/axios.ts";
import {AxiosResponse} from "axios";
import {ChallengeMode} from "../providers/AuthProvider.tsx";

const authApiPrefix = import.meta.env.VITE_AUTH_API_BASE_PATH;
const accountApiPrefix = import.meta.env.VITE_ACCOUNT_API_BASE_PATH;

const signUp = (data: SignUpRequest): Promise<SignUpResponse> => {
    return axios.post<SignUpResponse>(authApiPrefix + '/signup', data).then((res) => {
        return res.data;
    });
};

const signIn = async (data: SignInRequest): Promise<AuthUser> => {
    return axios.post<TokenResponse>(authApiPrefix + '/signin', data)
        .then((tokenRes) => {
            const user = {email: data.email} as AuthUser;
            storage.setUser(user);
            storage.setToken(tokenRes.data.access_token);
            return user;
        });
};

const externalChallenge = async (provider: string, callbackUrl: string) => {
    const api = import.meta.env.VITE_API_URL + authApiPrefix + "/external/challenge/" + provider
        + "?CallbackUrl=" + callbackUrl;
    window.location.replace(api);
};

const signUpExternal = async (data: SignUpExternalRequest): Promise<AuthUser | AxiosResponse> => {
    const path = '/signup/external';

    return axios.post<TokenResponse>(authApiPrefix + path, data).then((tokenRes) => {
        storage.setToken(tokenRes.data.access_token);
        return axios.get<WhoAmIResponse>(authApiPrefix + '/whoami').then((res) => {
            const user = {email: res.data.email} as AuthUser;
            storage.setUser(user);
            return user;
        })
    });
};

const signInExternal = async (): Promise<AuthUser | AxiosResponse> => {
    const path = '/signin/external';

    return axios.post<TokenResponse>(authApiPrefix + path).then((tokenRes) => {
        storage.setToken(tokenRes.data.access_token);
        return axios.get<WhoAmIResponse>(authApiPrefix + '/whoami').then((res) => {
            const user = {email: res.data.email} as AuthUser;
            storage.setUser(user);
            return user;
        })
    });
};

const linkLogin = async (): Promise<void | AxiosResponse> => {
    const path = '/link/external';

    return axios.post<void>(authApiPrefix + path);
};

const whoAmI = async (): Promise<WhoAmIResponse> => {
    return axios.get<WhoAmIResponse>(authApiPrefix + '/whoami').then(res => res.data);
};

const signOut = () => {
    storage.clearToken();
    storage.clearUser();
};

const refreshToken = (): Promise<TokenResponse> => {
    const ax = axiosCreate();
    return ax.post<TokenResponse>(authApiPrefix + '/signin/refresh').then((tokenRes) => {
        storage.setToken(tokenRes.data.access_token);
        return tokenRes.data;
    });
}

const forgotPassword = (email: string) => {
    return axios.post(accountApiPrefix + '/password/forgot', {email});
};

const resetPassword = (data: ResetPasswordRequest): Promise<AxiosResponse> => {
    return axios.post(accountApiPrefix + '/password/reset', data);
};

const changePassword = (data: ChangePasswordRequest): Promise<AxiosResponse> => {
    return axios.post(accountApiPrefix + '/password/change', data);
}

const resendEmailConfirmation = (email: string): Promise<void> => {
    return axios.post(accountApiPrefix + '/email/confirm/resend', {email});
}

const confirmEmail = (code: string, userId: string): Promise<AxiosResponse> => {
    return axios.get(accountApiPrefix + '/email/confirm', {
        params: {
            code,
            userId
        }
    });
}

const accountInfo = async () => {
    return axios.get<AccountInfoResponse>(accountApiPrefix + '/info').then(res => res.data);
}

export const auth = {
    signUp,
    signIn,
    challengeExternal: externalChallenge,
    signInExternal,
    signUpExternal,
    linkLogin,
    whoAmI,
    signOut,
    refreshToken,
    forgotPassword,
    resetPassword,
    confirmEmail,
    resendEmailConfirmation,
    accountInfo,
    changePassword
} as AuthApi;

export interface AuthApi {
    signUp: (data: SignUpRequest) => Promise<SignUpResponse>;
    signIn: (data: SignInRequest) => Promise<AuthUser>;
    challengeExternal: (provider: string, callbackUrl: string) => void;
    linkLogin: () => Promise<void | AxiosResponse>;
    signInExternal: () => Promise<AuthUser | AxiosResponse>;
    signUpExternal: (data: SignUpExternalRequest) => Promise<AuthUser | AxiosResponse>;
    whoAmI: () => Promise<WhoAmIResponse>;
    signOut: () => void;
    refreshToken: () => Promise<TokenResponse>;
    forgotPassword: (email: string) => Promise<AxiosResponse>;
    resetPassword: (data: ResetPasswordRequest) => Promise<AxiosResponse>;
    confirmEmail: (code: string, userId: string) => Promise<AxiosResponse>;
    resendEmailConfirmation: (email: string) => Promise<void>;
    accountInfo: () => Promise<AccountInfoResponse>;
    changePassword: (data: ChangePasswordRequest) => Promise<AxiosResponse>;
}

export interface AuthUser {
    email: string;
}

export interface SignUpRequest {
    email: string;
    password: string;
    
    // Optional fields, used for extended server example
    aboutMe?: string;
    mySiteUrl?: string;
}

export interface SignUpResponse {
    requiresEmailConfirmation: boolean;
}

export interface SignUpExternalRequest {
    // Optional fields, used for extended server example
    aboutMe?: string;
    mySiteUrl?: string;
}

export interface SignInRequest {
    email: string;
    password: string;
}

export interface ExternalChallengeRequest {
    mode: ChallengeMode;
    provider: string;
}

export interface WhoAmIResponse {
    username: string;
    email: string;
    roles: string[];
}

export interface ResetPasswordRequest {
    email: string;
    password: string;
    code: string;
}

export interface ChangePasswordRequest {
    password: string;
    newPassword: string;
}

export interface AccountInfoResponse {
    email: string;
    userName: string;
    roles: string[];
    logins: string[];
    hasPassword: boolean;
}

export interface TokenResponse {
    access_token: string;
    expires_in: number;
}