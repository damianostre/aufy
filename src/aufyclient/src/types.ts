import { type AufyStorage } from './aufy-storage.js';
import { type AxiosInstance } from 'axios';

export interface AufyClientOptions {
    apiBaseUrl: string;
    axiosInstance: AxiosInstance;
    authApiPrefix?: string;
    accountApiPrefix?: string;
    storage?: AufyStorage;
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
    provider: string;
    mode: ChallengeMode;
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