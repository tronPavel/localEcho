import { useNavigate } from 'react-router-dom';
import { RegisterForm } from '@/features/auth/ui/RegisterForm';
import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';

export const RegisterPage = () => {
    const navigate = useNavigate();

    return (
        <RoutedModal title="Создать аккаунт" maxWidth={450}>
            <RegisterForm
                onSuccess={() => navigate('/')}
                onSwitch={() => navigate('/login')}
            />
        </RoutedModal>
    );
};