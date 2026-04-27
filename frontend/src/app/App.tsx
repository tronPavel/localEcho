import { Routes, Route, Navigate } from 'react-router-dom';
import {usePermissions, useRestoreAuth} from '@/features/auth/model/authStore';

import { MapPage } from '@/pages/map/ui/MapPage';
import { LoginPage } from '@/pages/auth/LoginPage';
import { RegisterPage } from '@/pages/auth/RegisterPage';
import { ProfilePage } from '@/pages/profile/ProfilePage';
import { LeaderboardPage } from '@/pages/leaderboard/LeaderboardPage';
import { MarkerDetailsPage } from '@/pages/marker-details/MarkerDetailsPage';
import {CreateMarkerPage} from "@/pages/create-marker/CreateMarkerPage.tsx";
import {MarkerResolvePage} from "@/pages/marker-details/MarkerResolvePage.tsx";
import {DistrictDetailsPage} from "@/pages/district-details/DistrictDetailsPage.tsx";
import {Toaster} from "sonner";
import {ProtectedRoute} from "@/app/providers/ProtectedRoute.tsx";
import {ReportMarkerPage} from "@/pages/marker-details/ReportMarkerPage.tsx";
import {DashboardPage} from "@/pages/dashboard/DashboardPage.tsx";
import {ModerationTab} from "@/pages/dashboard/tabs/ModerationTab.tsx";
import {UserManagementTab} from "@/pages/dashboard/tabs/UserManagementTab.tsx";
import {OfficialTasksTab} from "@/pages/dashboard/tabs/OfficialTasksTab.tsx";
import {DistrictManagementTab} from "@/pages/dashboard/tabs/DistrictManagementTab.tsx";
import {AnalyticsPage} from "@/pages/analytics/AnalyticsPage.tsx";

const App = () => {
    useRestoreAuth();
    const DashboardIndexRedirect = () => {
        const { isAdmin, isModerator, isOfficial } = usePermissions();

        // Админ в приоритете идет на управление юзерами
        if (isAdmin) return <Navigate to="users" replace />;

        // Модератор — на жалобы
        if (isModerator) return <Navigate to="reports" replace />;

        // Официальное лицо — на задачи
        if (isOfficial) return <Navigate to="tasks" replace />;

        return <Navigate to="/" replace />;
    };
    return (
        <>
            <Toaster position="bottom-right" richColors closeButton duration={3000} />
            <Routes>
                <Route path="/" element={<MapPage />}>
                    <Route path="login" element={<LoginPage />} />
                    <Route path="register" element={<RegisterPage />} />
                    <Route path="leaderboard" element={<LeaderboardPage />} />
                    <Route path="marker/:id" element={<MarkerDetailsPage />} />
                    <Route path="districts/:id" element={<DistrictDetailsPage />} />
                    <Route path="analytics" element={<AnalyticsPage />} />

                    <Route path="/dashboard" element={
                        <ProtectedRoute requiredPermission="canAccessDashboard">
                            <DashboardPage />
                        </ProtectedRoute>
                    }>
                        <Route index element={<DashboardIndexRedirect />} />

                        <Route path="reports" element={<ModerationTab />} />
                        <Route path="users" element={<UserManagementTab />} />
                        <Route path="tasks" element={<OfficialTasksTab />} />
                        <Route path="districts" element={<DistrictManagementTab />} />
                    </Route>
                    <Route path="profile" element={
                        <ProtectedRoute>
                            <ProfilePage />
                        </ProtectedRoute>
                    } />

                    <Route path="create-marker" element={
                        <ProtectedRoute>
                            <CreateMarkerPage />
                        </ProtectedRoute>
                    } />

                    <Route path="marker/:id/resolve" element={
                        <ProtectedRoute requiredPermission="canResolveMarkers">
                            <MarkerResolvePage />
                        </ProtectedRoute>
                    } />
                    <Route path="marker/:id/report" element={
                        <ProtectedRoute>
                            <ReportMarkerPage />
                        </ProtectedRoute>
                    } />

                </Route>

                <Route path="*" element={<Navigate to="/" replace />} />
            </Routes>
        </>
    );
};

export default App;