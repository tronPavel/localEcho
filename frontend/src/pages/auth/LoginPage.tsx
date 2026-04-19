import { useNavigate } from 'react-router-dom';
import { LoginForm } from '@/features/auth/ui/LoginForm';
import { RoutedModal } from '@/shared/ui/Modal/RoutedModal';

export const LoginPage = () => {
    const navigate = useNavigate();

    return (
        <RoutedModal title="Вход в систему" >
            <LoginForm
                onSuccess={() => navigate('/')}
                onSwitch={() => navigate('/register')}
            />
        </RoutedModal>
    );
};