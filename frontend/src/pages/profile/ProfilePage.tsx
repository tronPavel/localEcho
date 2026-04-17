import {ProfileFeature} from "@/features/auth/ui/ProfileFeature.tsx";
import {useNavigate} from "react-router-dom";
import {RoutedModal} from "@/shared/ui/Modal/RoutedModal.tsx";

export const ProfilePage = () => {
    const navigate = useNavigate();
    return (
        <RoutedModal title="Мой профиль">
            <ProfileFeature onActionSuccess={() => navigate('/')} />
        </RoutedModal>
    );
}