import { Routes, Route, Navigate } from 'react-router-dom';
import { useRestoreAuth } from '@/features/auth/model/authStore';

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

const App = () => {
    useRestoreAuth();

    return (
        <>
            <Toaster
                position="bottom-right"
                richColors
                closeButton
                duration={3000}
            />
        <Routes>
            <Route path="/" element={<MapPage />}>
                <Route path="login" element={<LoginPage />} />
                <Route path="register" element={<RegisterPage />} />
                <Route path="profile" element={<ProfilePage />} />
                <Route path="leaderboard" element={<LeaderboardPage />} />
                <Route path="marker/:id" element={<MarkerDetailsPage />} />
                <Route path="marker/:id/resolve" element={<MarkerResolvePage />} />
                <Route path="create-marker" element={<CreateMarkerPage />} />
                 <Route path="districts/:id" element={<DistrictDetailsPage />} />

            </Route>

            <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
        </>
    );
};

export default App;