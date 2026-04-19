import { CreateMarkerForm } from '@/features/create-marker/ui/CreateMarkerForm';
import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';
import { useNavigate } from 'react-router-dom';

export const CreateMarkerPage = () => {
    const navigate = useNavigate();

    return (
        <RoutedModal title="Создание новой метки">
            <CreateMarkerForm onSuccess={() => navigate('/')} />
        </RoutedModal>
    );
};