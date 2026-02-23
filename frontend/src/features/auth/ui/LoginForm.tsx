import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/ui/Button/Button.tsx';
import { Input } from '@/shared/ui/Input/Input.tsx';
import { loginSchema } from '../lib/validateAuth';
import { useMutation } from '@tanstack/react-query';
import { login } from '../model/authApi';
import { useAuthStore } from '../model/authStore';
import styles from './AuthForms.module.css';

export const LoginForm = ({ onSuccess, onSwitch }: { onSuccess: () => void; onSwitch: () => void }) => {
    const { register, handleSubmit, formState: { errors } } = useForm({
        resolver: zodResolver(loginSchema),
    });
    const { setUser } = useAuthStore();
    const mutation = useMutation({
        mutationFn: login,
        onSuccess: (data) => {
            setUser(data);
            onSuccess();
        },
    });

    return (
        <form onSubmit={handleSubmit((data) => mutation.mutate(data))} className={styles.form}>
            <Input placeholder="Email" {...register('email')} error={errors.email?.message as string} />
            <Input type="password" placeholder="Пароль" {...register('password')} error={errors.password?.message as string} />
            <Button type="submit" disabled={mutation.isPending}>Войти</Button>
            <p className={styles.switch}>Нет аккаунта? <span onClick={onSwitch}>Зарегистрироваться</span></p>
        </form>
    );
};