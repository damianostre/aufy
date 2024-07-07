import Axios, {AxiosError} from 'axios';

export const axiosCreate = () => {
    return Axios.create({
        baseURL: import.meta.env.VITE_API_URL,
        withCredentials: true,
        headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json',
        },
    });
}

export const axios = axiosCreate();
export const extractApiErrors = (error: any) => {
    if (!Axios.isAxiosError(error)) return null;

    const e = error as AxiosError;
    if (!e.response?.data) return null;
    
    const data = e.response.data as any;
    if (!data.errors) return null;
    
    const errors = data.errors as any;
    return Object.keys(errors).flatMap(key => errors[key]);
}

