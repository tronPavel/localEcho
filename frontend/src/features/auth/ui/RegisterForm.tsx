import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation, useQuery } from '@tanstack/react-query';
import { toast } from 'sonner';

import { Input } from '@/shared/ui/Input/Input';
import { Select } from '@/shared/ui/Select/Select';
import { Button } from '@/shared/ui/Button/Button';

import { registerSchema } from '../lib/validateAuth';
import { register as registerApi } from '../model/authApi';
import { useAuthStore } from '../model/authStore';
import { cityApi } from '@/entities/city/api/cityApi';
import type { RegisterDto } from '../model/types';

import styles from './AuthForms.module.css';
import {districtApi} from "@/entities/district/model/districtApi.ts";

interface RegisterFormProps {
    onSuccess: () => void;
    onSwitch: () => void;
}

export const RegisterForm = ({ onSuccess, onSwitch }: RegisterFormProps) => {
    const { setUser } = useAuthStore();

    const {
        register,
        handleSubmit,
        watch,
        formState: { errors }
    } = useForm<RegisterDto>({
        resolver: zodResolver(registerSchema),
        defaultValues: {
            cityId: '',
            districtId: ''
        }
    });

    const selectedCityId = watch('cityId');

    const { data: cities = [] } = useQuery({
        queryKey: ['cities-list'],
        queryFn: cityApi.getList
    });

    const { data: districts = [], isFetching: isDistrictsLoading } = useQuery({
        queryKey: ['districts-by-city', selectedCityId],
        queryFn: () => districtApi.getList(selectedCityId),
        enabled: !!selectedCityId,
    });

    const mutation = useMutation({
        mutationFn: registerApi,
        onSuccess: (data) => {
            setUser(data);
            toast.success(`Рады знакомству, ${data.name}!`);
            onSuccess();
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.detail || "Ошибка регистрации");
        }
    });

    return (
        <form onSubmit={handleSubmit((d) => mutation.mutate(d))} className={styles.form}>
            <div className={styles.registerGrid}>
                <div className={styles.column}>
                    <Input
                        label="Email"
                        placeholder="example@mail.com"
                        {...register('email')}
                        error={errors.email?.message}
                    />

                    <Input
                        label="Ваше имя"
                        placeholder="ivan_ivanov"
                        {...register('name')}
                        error={errors.name?.message}
                    />

                    <Select
                        label="Город"
                        {...register('cityId')}
                        error={errors.cityId?.message}
                    >
                        <option value="" disabled>Выберите город</option>
                        {cities.map((c) => (
                            <option key={c.id} value={c.id}>{c.name}</option>
                        ))}
                    </Select>
                </div>

                <div className={styles.column}>
                    <Select
                        label="Район"
                        {...register('districtId')}
                        disabled={!selectedCityId}
                    >
                        <option value="">{isDistrictsLoading ? 'Загрузка...' : 'Весь город / Не выбрано'}</option>
                        {districts.map((d) => (
                            <option key={d.id} value={d.id}>{d.name}</option>
                        ))}
                    </Select>

                    <Input
                        label="Пароль"
                        type="password"
                        {...register('password')}
                        error={errors.password?.message}
                    />

                    <Input
                        label="Повторите пароль"
                        type="password"
                        {...register('confirmPassword')}
                        error={errors.confirmPassword?.message}
                    />
                </div>
            </div>

            <div className={styles.formFooter}>
                <Button type="submit" disabled={mutation.isPending}>
                    {mutation.isPending ? 'Загрузка...' : 'Зарегистрироваться'}
                </Button>
                <p className={styles.switch}>
                    Уже есть аккаунт? <span onClick={onSwitch}>Войти</span>
                </p>
            </div>
        </form>
    );
};