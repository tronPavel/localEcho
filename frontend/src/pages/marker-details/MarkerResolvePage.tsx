import { useParams } from 'react-router-dom';
import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';
import { MarkerResolveFeature } from '@/features/view-marker/ui/MarkerResolveFeature';
import { usePermissions } from '@/features/auth/model/authStore';
import { Navigate } from 'react-router-dom';

export const MarkerResolvePage = () => {
    const { id } = useParams();
    const { canResolveMarkers } = usePermissions();

    if (!canResolveMarkers) return <Navigate to={`/marker/${id}`} replace />;
    if (!id) return null;

    return (
        <RoutedModal title="Официальное решение задачи">
            <MarkerResolveFeature markerId={id} />
        </RoutedModal>
    );
};