import { MapPage } from '@/pages/map/ui/MapPage';
import { useRestoreAuth } from '@/features/auth/model/authStore';

function App() {
    useRestoreAuth();
    return <MapPage />;
}

export default App;