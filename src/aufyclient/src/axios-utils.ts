import Axios, { AxiosError, type CreateAxiosDefaults } from 'axios';

export const createAxiosInstance = (apiBaseUrl: string, config?: CreateAxiosDefaults) => {
    return Axios.create({
        baseURL: apiBaseUrl,
        withCredentials: true,

        ...config,
    });
};

export const extractApiErrors = (error: any) => {
    if (!Axios.isAxiosError(error)) return null;

    const e = error as AxiosError;
    if (!e.response?.data) return null;

    const data = e.response.data as any;
    if (!data.errors) return null;

    const errors = data.errors as any;
    return Object.keys(errors).flatMap(key => errors[key]);
};

