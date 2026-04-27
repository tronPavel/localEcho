import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';
import { useNavigate } from 'react-router-dom';
import {MarkerCreateForm} from "@/features/marker-actions/create";

export const CreateMarkerPage = () => {
    const navigate = useNavigate();

    return (
        <RoutedModal title="Создание новой метки">
            <MarkerCreateForm onSuccess={() => navigate('/')} />
        </RoutedModal>
    );
};