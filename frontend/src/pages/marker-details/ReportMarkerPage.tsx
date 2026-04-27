import { useParams, useNavigate } from 'react-router-dom';
import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';
import {ReportForm} from "@/features/marker-actions/report/ui/ReportForm.tsx";

export const ReportMarkerPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();

    if (!id) return null;

    return (
        <RoutedModal title="Пожаловаться на метку">
            <ReportForm
                markerId={id}
                onSuccess={() => navigate(`/marker/${id}`)} // Возвращаем в детали после успеха
            />
        </RoutedModal>
    );
};