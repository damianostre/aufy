import * as React from 'react';
import {ErrorBoundary} from 'react-error-boundary';
import {HelmetProvider} from 'react-helmet-async';
import {BrowserRouter as Router} from 'react-router-dom';
import {AuthProvider} from "./AuthProvider.tsx";

const ErrorFallback = () => {
    return (
        <div
            className="text-red-500 w-screen h-screen flex flex-col justify-center items-center"
            role="alert"
        >
            <h2 className="text-lg font-semibold">Ooops, something went wrong :( </h2>
            <button className="btn" onClick={() => window.location.assign(window.location.origin)}>
                Refresh
            </button>
        </div>
    );
};

type AppProviderProps = {
    children: React.ReactNode;
};

export const AppProvider = ({children}: AppProviderProps) => {
    return (
        <React.Suspense
            fallback={
                <div className="flex items-center justify-center w-screen h-screen">
                    ...loading
                </div>
            }
        >
            <ErrorBoundary FallbackComponent={ErrorFallback}>
                <HelmetProvider>
                    <AuthProvider>
                        <Router>{children}</Router>
                    </AuthProvider>
                </HelmetProvider>
            </ErrorBoundary>
        </React.Suspense>
    );
};
