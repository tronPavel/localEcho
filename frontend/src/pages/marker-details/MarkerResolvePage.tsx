import { useParams, useNavigate } from 'react-router-dom';
import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';
import { MarkerResolveForm } from '@/features/marker-actions/resolve';

export const MarkerResolvePage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    return (
        <RoutedModal title="Официальный ответ">
            <MarkerResolveForm
                markerId={id!}
                onSuccess={() => navigate(`/marker/${id}`)}
                onCancel={() => navigate(-1)}
            />
        </RoutedModal>
    );
};