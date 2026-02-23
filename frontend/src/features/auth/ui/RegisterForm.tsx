import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Input } from '@/shared/ui/Input/Input.tsx';
import { Select } from '@/shared/ui/Select/Select.tsx';
import { registerSchema } from '../lib/validateAuth';
import { useMutation, useQuery } from '@tanstack/react-query';
import { getDistricts } from "@/entities/district/model/districtApi"; // <-- Новый путь
import { register } from "../model/authApi";
import { useAuthStore } from '../model/authStore';
import styles from './AuthForms.module.css';
import { Button } from "@/shared/ui/Button/Button.tsx";
import type { RegisterDto, AuthResponseDto, DistrictDto } from '../model/types';

export const RegisterForm = ({ onSuccess, onSwitch }: { onSuccess: () => void; onSwitch: () => void }) => {
    const { register: formRegister, handleSubmit, formState: { errors } } = useForm<RegisterDto>({
        resolver: zodResolver(registerSchema),
    });
    const { data: districts = [] } = useQuery<DistrictDto[]>({ queryKey: ['districts'], queryFn: getDistricts });
    const { setUser } = useAuthStore();
    const mutation = useMutation<AuthResponseDto, Error, RegisterDto>({
        mutationFn: register,
        onSuccess: (data) => {
            setUser(data);
            onSuccess();
        },
    });

    return (
        <form onSubmit={handleSubmit((data) => mutation.mutate(data))} className={styles.form}>
            <Input placeholder="Email" {...formRegister('email')} error={errors.email?.message as string} />
            <Input placeholder="Имя" {...formRegister('name')} error={errors.name?.message as string} />
            <Select {...formRegister('districtId')}>
                <option value="">Выберите район</option>
                {districts.map((d: DistrictDto) => <option key={d.id} value={d.id}>{d.name}</option>)}
            </Select>
            {errors.districtId && <span className={styles.error}>{errors.districtId.message}</span>}
            <Input placeholder="Адрес" {...formRegister('homeAddress')} />
            <Input type="password" placeholder="Пароль" {...formRegister('password')} error={errors.password?.message as string} />
            <Input type="password" placeholder="Подтвердите пароль" {...formRegister('confirmPassword')} error={errors.confirmPassword?.message as string} />
            <Button type="submit" disabled={mutation.isPending}>Зарегистрироваться</Button>
            <p className={styles.switch}>Уже есть аккаунт? <span onClick={onSwitch}>Войти</span></p>
        </form>
    );
};