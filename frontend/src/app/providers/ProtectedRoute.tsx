import type { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { usePermissions } from '@/features/auth/model/authStore';

interface ProtectedRouteProps {
    children: ReactNode;
    requiredPermission?: 'canDrawPolygons' | 'canResolveMarkers' | 'canAccessDashboard';
}

export const ProtectedRoute = ({ children, requiredPermission }: ProtectedRouteProps) => {
    const permissions = usePermissions();
    const location = useLocation();

    if (!permissions.isAuthenticated) {
        return <Navigate to="/login" state={{ from: location }} replace />;
    }

    if (requiredPermission && !permissions[requiredPermission]) {
        console.warn(`Access denied: required ${requiredPermission}`);
        return <Navigate to="/" replace />;
    }

    return <>{children}</>;
};