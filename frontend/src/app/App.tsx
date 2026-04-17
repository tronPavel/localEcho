import { Routes, Route, Navigate } from 'react-router-dom';
import { useRestoreAuth } from '@/features/auth/model/authStore';

// Импорты страниц (Pages)
import { MapPage } from '@/pages/map/ui/MapPage';
import { LoginPage } from '@/pages/auth/LoginPage';
import { RegisterPage } from '@/pages/auth/RegisterPage';
import { ProfilePage } from '@/pages/profile/ProfilePage';
import { LeaderboardPage } from '@/pages/leaderboard/LeaderboardPage';
import { MarkerDetailsPage } from '@/pages/marker-details/MarkerDetailsPage';

const App = () => {
    useRestoreAuth();

    return (
        <Routes>
            <Route path="/" element={<MapPage />}>
                <Route path="login" element={<LoginPage />} />
                <Route path="register" element={<RegisterPage />} />
                <Route path="profile" element={<ProfilePage />} />
                <Route path="leaderboard" element={<LeaderboardPage />} />
                <Route path="marker/:id" element={<MarkerDetailsPage />} />
            </Route>

            <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
    );
};

export default App;