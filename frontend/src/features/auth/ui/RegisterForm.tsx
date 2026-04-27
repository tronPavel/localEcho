import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery } from '@tanstack/react-query';
import { toast } from 'sonner';

import { Input } from '@/shared/ui/Input/Input';
import { Select } from '@/shared/ui/Select/Select';
import { Button } from '@/shared/ui/Button/Button';

import { registerSchema } from '../lib/validateAuth';
import { register as registerUserApi } from '../model/authApi';
import { useAuthStore } from '../model/authStore';
import { districtApi} from "@/entities/district/model/districtApi";
import type { RegisterDto } from '../model/types';

import styles from './AuthForms.module.css';

interface RegisterFormProps {
    onSuccess: () => void;
    onSwitch: () => void;
}

export const RegisterForm = ({ onSuccess, onSwitch }: RegisterFormProps) => {
    const { setUser } = useAuthStore();
    const [isLocating, setIsLocating] = useState(false);

    const {
        register: control,
        handleSubmit,
        setValue,
        setError,
        formState: { errors }
    } = useForm<RegisterDto>({
        resolver: zodResolver(registerSchema),
    });

    const { data: districts = [] } = useQuery({
        queryKey: ['districts-list'],
        queryFn: districtApi.getList,
    });

    const mutation = useMutation({
        mutationFn: registerUserApi,
        onSuccess: (data) => {
            setUser(data);
            toast.success(`Рады знакомству, ${data.name}! ✨`);
            onSuccess();
        },
        onError: (err: any) => {
            const serverErrors = err.response?.data?.errors;
            if (serverErrors) {
                // Мапим ошибки сервера на поля формы
                Object.keys(serverErrors).forEach((key) => {
                    setError(key as keyof RegisterDto, {
                        type: "server",
                        message: serverErrors[key][0]
                    });
                });
            } else {
                toast.error(err.response?.data?.detail || "Ошибка регистрации");
            }
        }
    });

    const handleAutoDetect = () => {
        if (!("geolocation" in navigator)) return toast.error("GPS не поддерживается");

        setIsLocating(true);
        navigator.geolocation.getCurrentPosition(
            async (pos) => {
                try {
                    const result = await districtApi.findByCoords(pos.coords.latitude, pos.coords.longitude);
                    setValue('districtId', result.id, { shouldValidate: true });
                    toast.success(`Ваш район: ${result.name}`);
                } catch (e) {
                    toast.info("Местоположение вне зон обслуживания. Вы в режиме гостя.");
                } finally {
                    setIsLocating(false);
                }
            },
            () => {
                toast.error("Доступ к GPS отклонен");
                setIsLocating(false);
            }
        );
    };

    return (
        <form onSubmit={handleSubmit((d) => mutation.mutate(d))} className={styles.form}>
            <div className={styles.registerGrid}>
                {/* Левая колонка */}
                <div className={styles.column}>
                    <Input
                        label="Электронная почта"
                        placeholder="minsk@example.by"
                        {...control('email')}
                        error={errors.email?.message}
                    />
                    <Input
                        label="Никнейм"
                        placeholder="minsk_pioneer"
                        {...control('name')}
                        error={errors.name?.message}
                    />
                    <div className={styles.districtContainer}>
                        <Select label="Ваш район" {...control('districtId')} error={errors.districtId?.message}>
                            <option value="">Не привязан (Гость)</option>
                            {districts.map((d) => <option key={d.id} value={d.id}>{d.name}</option>)}
                        </Select>
                        <Button
                            type="button"
                            variant="outline"
                            className={styles.locateBtn}
                            onClick={handleAutoDetect}
                            disabled={isLocating}
                        >
                            {isLocating ? "⌛" : "🎯"}
                        </Button>
                    </div>
                </div>

                {/* Правая колонка */}
                <div className={styles.column}>
                    <Input
                        label="Пароль"
                        type="password"
                        placeholder="Минимум 6 знаков"
                        {...control('password')}
                        error={errors.password?.message}
                    />
                    <Input
                        label="Подтверждение"
                        type="password"
                        placeholder="Повторите пароль"
                        {...control('confirmPassword')}
                        error={errors.confirmPassword?.message}
                    />
                    <Input
                        label="Домашний адрес"
                        placeholder="Напр: ул. Независимости, 10"
                        {...control('homeAddress')}
                    />
                </div>
            </div>

            <div className={styles.formFooter}>
                <Button type="submit" disabled={mutation.isPending}>
                    {mutation.isPending ? 'Создаем...' : 'Стать частью сообщества'}
                </Button>
                <p className={styles.switch}>
                    Уже зарегистрированы? <span onClick={onSwitch}>Войти</span>
                </p>
            </div>
        </form>
    );
};