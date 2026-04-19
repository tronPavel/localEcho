import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/shared/ui/Button/Button.tsx';
import { Input } from '@/shared/ui/Input/Input.tsx';
import { loginSchema } from '../lib/validateAuth';
import { useMutation } from '@tanstack/react-query';
import { login } from '../model/authApi';
import { useAuthStore } from '../model/authStore';
import styles from './AuthForms.module.css';
import {toast} from "sonner";

export const LoginForm = ({ onSuccess, onSwitch }: { onSuccess: () => void; onSwitch: () => void }) => {
    const { register, handleSubmit, formState: { errors } } = useForm({
        resolver: zodResolver(loginSchema),
    });
    const { setUser } = useAuthStore();
    const mutation = useMutation({
        mutationFn: login,
        onSuccess: (data) => {
            setUser(data);
            toast.success(`Рады видеть вас снова, ${data.name}!`);
            onSuccess();
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.detail || "Неверный логин или пароль");
        }
    });

    return (
        <form onSubmit={handleSubmit((data) => mutation.mutate(data))} className={styles.form}>
            <Input
                label="Электронная почта"
                placeholder="example@mail.com"
                {...register('email')}
                error={errors.email?.message as string}
            />
            <Input
                label="Пароль"
                type="password"
                {...register('password')}
                error={errors.password?.message as string}
            />

            <div className={styles.formFooter}>
                <Button type="submit" disabled={mutation.isPending}>
                    {mutation.isPending ? 'Вход...' : 'Войти в аккаунт'}
                </Button>
                <p className={styles.switch}>
                    Впервые у нас? <span onClick={onSwitch}>Создать профиль</span>
                </p>
            </div>
        </form>
    );
};