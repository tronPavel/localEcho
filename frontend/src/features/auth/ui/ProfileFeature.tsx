import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';

import { Button } from '@/shared/ui/Button/Button';
import { Input } from '@/shared/ui/Input/Input';
import { Select } from '@/shared/ui/Select/Select';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { ImageUploader } from '@/shared/ui/ImageUploader/ImageUploader';

import { getMyProfile, updateProfile } from '@/entities/user/model/userApi';
import { getDistrictsList } from '@/entities/district/model/districtApi';
import { logout } from '../model/authApi';
import { useAuthStore } from '../model/authStore';
import type { UpdateProfileDto } from '@/entities/user/model/types';

import styles from './ProfileFeature.module.css';

interface ProfileFeatureProps {
    onActionSuccess: () => void;
}

export const ProfileFeature = ({ onActionSuccess }: ProfileFeatureProps) => {
    const { clearUser } = useAuthStore();
    const queryClient = useQueryClient();

    const [editMode, setEditMode] = useState(false);
    const [avatarFile, setAvatarFile] = useState<File | undefined>();

    // 1. Получение данных
    const { data: profile, refetch, isLoading: isProfileLoading } = useQuery({
        queryKey: ['profile'],
        queryFn: getMyProfile
    });

    const { data: districts = [] } = useQuery({
        queryKey: ['districts-list'],
        queryFn: getDistrictsList
    });

    const { register, handleSubmit, setValue, formState: { errors } } = useForm<UpdateProfileDto>();

    // 2. Логика Logout (Решение ошибок TS6133/TS2304)
    const handleLogout = async () => {
        try {
            await logout(); // API вызов
            clearUser();   // Очистка стора
            toast.info("Вы вышли из аккаунта");
            onActionSuccess(); // Переход navigate('/')
        } catch (e) {
            toast.error("Проблема при выходе из системы");
            clearUser();
            onActionSuccess();
        }
    };

    useEffect(() => {
        if (profile && editMode) {
            setValue('name', profile.name);
            setValue('homeAddress', profile.homeAddress);
            setValue('districtId', profile.district?.id);
        }
    }, [profile, editMode, setValue]);

    const mutation = useMutation({
        mutationFn: (data: UpdateProfileDto) => updateProfile({ ...data, avatarFile }),
        onSuccess: () => {
            setEditMode(false);
            setAvatarFile(undefined);
            queryClient.invalidateQueries({ queryKey: ['profile'] });
            toast.success("Изменения успешно применены ✨");
            refetch();
        },
        onError: (err: any) => {
            const message = err.response?.data?.detail || "Ошибка при сохранении";
            toast.error(message);
        }
    });

    if (isProfileLoading) return <div className={styles.loading}>Загружаем ваш профиль...</div>;
    if (!profile) return null;

    return (
        <div className={styles.profileContainer}>
            {!editMode ? (
                /* --- РЕЖИМ ПРОСМОТРА --- */
                <>
                    <header className={styles.topInfo}>
                        <UserAvatar user={profile} size="large" />
                        <div className={styles.nameSection}>
                            <div className={styles.nameLine}>
                                <h1 className={styles.userName}>{profile.name}</h1>
                            </div>
                            <span className={styles.userEmail}>{profile.email}</span>
                        </div>
                    </header>

                    <div className={styles.statsGrid}>
                        <div className={styles.statCard}>
                            <span className={styles.statLabel}>Карма Минска</span>
                            <span className={styles.statValue}>{profile.points} pts</span>
                        </div>
                        <div className={styles.statCard}>
                            <span className={styles.statLabel}>Район проживания</span>
                            <span className={styles.statValue}>
                                {profile.district?.name || 'Гость (не привязан)'}
                            </span>
                        </div>
                    </div>

                    {profile.homeAddress && (
                        <div className={styles.addressSection}>
                            <label>Ваш текущий адрес:</label>
                            <p>📍 {profile.homeAddress}</p>
                        </div>
                    )}

                    <div className={styles.actionRow}>
                        <Button onClick={() => setEditMode(true)}>🔧 Настроить данные</Button>
                        <Button variant="outline" onClick={handleLogout}>🚪 Выход</Button>
                    </div>
                </>
            ) : (
                <form onSubmit={handleSubmit(d => mutation.mutate(d))} className={styles.editForm}>
                    <div className={styles.formGrid}>
                        <div className={styles.column}>
                            <Input
                                label="Публичное имя"
                                {...register('name', { required: 'Имя не может быть пустым' })}
                                error={errors.name?.message}
                            />

                            <Select label="Принадлежность к району" {...register('districtId')}>
                                <option value="">Оставить пустым (Гость)</option>
                                {districts.map(d => (
                                    <option key={d.id} value={d.id}>{d.name}</option>
                                ))}
                            </Select>

                            <Input
                                label="Домашний адрес"
                                {...register('homeAddress')}
                                placeholder="Напр: ул. Независимости, 10"
                            />
                            <p className={styles.formHint}>
                                Адрес проверяется на вхождение в границы выбранного района.
                            </p>
                        </div>

                        <div className={styles.column}>
                            <ImageUploader
                                label="Фотография профиля"
                                multiple={false}
                                initialPreview={profile.avatarUrl}
                                onFilesChange={(files) => setAvatarFile(files[0])}
                            />
                        </div>
                    </div>

                    <div className={styles.editFooter}>
                        <Button variant="secondary" type="button" onClick={() => setEditMode(false)}>
                            Назад
                        </Button>
                        <Button type="submit" disabled={mutation.isPending}>
                            {mutation.isPending ? 'Сохранение...' : 'Обновить профиль'}
                        </Button>
                    </div>
                </form>
            )}
        </div>
    );
};