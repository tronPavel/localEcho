import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Input } from '@/shared/ui/Input/Input.tsx';
import { Select } from '@/shared/ui/Select/Select.tsx';
import { Button } from "@/shared/ui/Button/Button.tsx";
import { registerSchema } from '../lib/validateAuth';
import { useMutation, useQuery } from '@tanstack/react-query';
import { getDistricts, findDistrictByCoords } from "@/entities/district/model/districtApi";
import { register } from "../model/authApi";
import { useAuthStore } from '../model/authStore';
import type { RegisterDto, AuthResponseDto, DistrictDto } from '../model/types';
import styles from './AuthForms.module.css';

export const RegisterForm = ({ onSuccess, onSwitch }: { onSuccess: () => void; onSwitch: () => void }) => {
    const { register: formRegister, handleSubmit, setValue, formState: { errors } } = useForm<RegisterDto>({
        resolver: zodResolver(registerSchema),
    });

    const [isLocating, setIsLocating] = useState(false);
    const { data: districts = [] } = useQuery({ queryKey: ['districts'], queryFn: getDistricts });
    const { setUser } = useAuthStore();

    const mutation = useMutation<AuthResponseDto, Error, RegisterDto>({
        mutationFn: register,
        onSuccess: (data) => {
            setUser(data);
            onSuccess();
        },
    });

    const handleAutoDetect = () => {
        if (!("geolocation" in navigator)) return alert("Геолокация недоступна");

        setIsLocating(true);
        navigator.geolocation.getCurrentPosition(
            async (pos) => {
                try {
                    const result = await findDistrictByCoords(pos.coords.latitude, pos.coords.longitude);
                    setValue('districtId', result.id, { shouldValidate: true });
                } catch (e) {
                    alert("Ваше местоположение не привязано к нашим районам");
                } finally {
                    setIsLocating(false);
                }
            },
            () => {
                alert("Ошибка доступа к GPS");
                setIsLocating(false);
            }
        );
    };

    return (
        <form onSubmit={handleSubmit((data) => mutation.mutate(data))} className={styles.form}>
            <Input placeholder="Email" {...formRegister('email')} error={errors.email?.message as string} />
            <Input placeholder="Имя" {...formRegister('name')} error={errors.name?.message as string} />

            <div className={styles.districtRow}>
                <Select {...formRegister('districtId')} style={{ flex: 1 }}>
                    <option value="">Выберите район</option>
                    {districts.map((d: DistrictDto) => <option key={d.id} value={d.id}>{d.name}</option>)}
                </Select>
                <Button
                    type="button"
                    variant="secondary"
                    onClick={handleAutoDetect}
                    disabled={isLocating}
                >
                    {isLocating ? "⌛" : "где я?"}
                </Button>
            </div>
            {errors.districtId && <span className={styles.error}>{errors.districtId.message}</span>}

            <Input placeholder="Домашний адрес (опционально)" {...formRegister('homeAddress')} />
            <Input type="password" placeholder="Пароль" {...formRegister('password')} error={errors.password?.message as string} />
            <Input type="password" placeholder="Повтор пароля" {...formRegister('confirmPassword')} error={errors.confirmPassword?.message as string} />

            <Button type="submit" disabled={mutation.isPending}>
                {mutation.isPending ? 'Загрузка...' : 'Зарегистрироваться'}
            </Button>
            <p className={styles.switch}>Уже есть аккаунт? <span onClick={onSwitch}>Войти</span></p>
        </form>
    );
};