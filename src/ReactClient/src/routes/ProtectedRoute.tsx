import React from "react";
import {Navigate, Outlet} from "react-router-dom";
import {useAuth} from "../providers/AuthProvider.tsx";

export const ProtectedRoute = ({ children } : {children: React.ReactNode} ) => {
    const { user } = useAuth();
    
    if (!user) {
        return <Navigate to={"/signin"} replace />;
    }

    return children ? children : <Outlet />;
};