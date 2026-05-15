import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';

import { Button } from '@/shared/ui/Button/Button';
import { Input } from '@/shared/ui/Input/Input';
import { Select } from '@/shared/ui/Select/Select';
import { Textarea } from '@/shared/ui/Textarea/Textarea';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { ImageUploader } from '@/shared/ui/ImageUploader/ImageUploader';

import { getMyProfile, updateProfile } from '@/entities/user/model/userApi';
import { cityApi } from '@/entities/city/api/cityApi';
import { logout } from '../model/authApi';
import { useAuthStore } from '../model/authStore';
import type { UpdateProfileDto } from '@/entities/user/model/types';

import styles from './ProfileFeature.module.css';
import {districtApi} from "@/entities/district/model/districtApi.ts";

interface ProfileFeatureProps {
    onActionSuccess: () => void; // Вызывается после логаута для перехода на главную
}

export const ProfileFeature = ({ onActionSuccess }: ProfileFeatureProps) => {
    const { clearUser } = useAuthStore();
    const queryClient = useQueryClient();
    const [editMode, setEditMode] = useState(false);
    const [avatarFile, setAvatarFile] = useState<File | undefined>();

    // 1. Данные текущего профиля
    const { data: profile, isLoading: isProfileLoading, refetch } = useQuery({
        queryKey: ['profile'],
        queryFn: getMyProfile,
    });

    // 2. Список городов (доступен всегда для формы)
    const { data: cities = [] } = useQuery({
        queryKey: ['cities-list'],
        queryFn: cityApi.getList,
        enabled: editMode,
    });

    const {
        register,
        handleSubmit,
        reset,
        watch,
        setValue,
        formState: { errors }
    } = useForm<UpdateProfileDto & { cityId?: string }>();

    // 3. Следим за полем выбора города
    const watchedCityId = watch('cityId');

    // 4. Подгружаем районы, когда город в форме изменился
    const { data: districts = [], isFetching: isDistrictsLoading } = useQuery({
        queryKey: ['districts-filtered', watchedCityId],
        queryFn: () => districtApi.getList(watchedCityId),
        enabled: !!watchedCityId && editMode,
    });

    // ФИКС 1: Сброс района при смене города в форме, чтобы не отправить невалидную связку
    useEffect(() => {
        if (editMode && watchedCityId && watchedCityId !== profile?.city?.id) {
            setValue('districtId', '');
        }
    }, [watchedCityId, setValue, editMode, profile?.city?.id]);

    // ФИКС 2: Первичная подстановка данных из API в форму при включении редактирования
    useEffect(() => {
        if (profile && editMode) {
            reset({
                name: profile.name,
                bio: profile.bio || '',
                homeAddress: profile.homeAddress || '',
                cityId: profile.city?.id || '',
                districtId: profile.district?.id || '',
            });
        }
    }, [profile, editMode, reset]);

    const handleLogout = async () => {
        try {
            await logout();
            clearUser();
            toast.info("Сессия завершена");
            onActionSuccess();
        } catch (e) {
            clearUser();
            onActionSuccess();
        }
    };

    const mutation = useMutation({
        mutationFn: (data: UpdateProfileDto) => updateProfile({ ...data, avatarFile }),
        onSuccess: () => {
            setEditMode(false);
            setAvatarFile(undefined);
            queryClient.invalidateQueries({ queryKey: ['profile'] });
            toast.success("Данные успешно обновлены ✨");
            refetch();
        },
        onError: (err: any) => {
            toast.error(err.response?.data?.detail || "Ошибка. Проверьте правильность адреса.");
        }
    });

    if (isProfileLoading) return <div className={styles.loading}>Загружаем профиль...</div>;
    if (!profile) return null;

    return (
        <div className={styles.profileContainer}>
            {!editMode ? (
                <>
                    <header className={styles.topInfo}>
                        <UserAvatar user={profile} size="large" />
                        <div className={styles.nameSection}>
                            <h1 className={styles.userName}>{profile.name}</h1>
                            <span className={styles.userEmail}>{profile.email}</span>
                        </div>
                    </header>

                    <div className={styles.sectionBox}>
                        <h3>О себе</h3>
                        {profile.bio ? (
                            <p className={styles.bioText}>{profile.bio}</p>
                        ) : (
                            <p className={styles.noData}>Вы еще не добавили информацию о себе.</p>
                        )}
                    </div>

                    <div className={styles.statsGrid}>
                        <div className={styles.statCard}>
                            <span className={styles.statLabel}>Город проживания</span>
                            <span className={styles.statValue}>{profile.city?.name || 'Не указан'}</span>
                        </div>
                        <div className={styles.statCard}>
                            <span className={styles.statLabel}>Район (Территория)</span>
                            <span className={styles.statValue}>{profile.district?.name || 'Гость (вне районов)'}</span>
                        </div>
                        <div className={styles.statCard}>
                            <span className={styles.statLabel}>Карма в системе</span>
                            <span className={styles.statValue}>{profile.points} pts</span>
                        </div>
                        <div className={styles.statCard}>
                            <span className={styles.statLabel}>Регистрация</span>
                            <span className={styles.statValue}>{new Date(profile.createdAt).toLocaleDateString()}</span>
                        </div>
                    </div>

                    <div className={styles.addressDisplay}>
                        <label>Моя домашняя точка</label>
                        <p>{profile.homeAddress || 'Точный адрес не привязан'}</p>
                    </div>

                    <div className={styles.actionRow}>
                        <div className={styles.leftBtns}>
                            <Button onClick={() => setEditMode(true)}>🔧 Редактировать</Button>
                        </div>
                        <Button variant="outline" className={styles.logoutBtn} onClick={handleLogout}>
                            Выйти из аккаунта
                        </Button>
                    </div>
                </>
            ) : (
                <form onSubmit={handleSubmit(d => mutation.mutate(d))} className={styles.editForm}>
                    <div className={styles.formGrid}>
                        <div className={styles.column}>
                            <Input
                                label="Никнейм"
                                {...register('name', { required: 'Имя не может быть пустым' })}
                                error={errors.name?.message}
                            />

                            <Textarea
                                label="О себе"
                                {...register('bio')}
                                placeholder="Напр.: Увлекаюсь урбанистикой, живу во Фрунзенском районе 10 лет..."
                                rows={4}
                            />

                            <div className={styles.row}>
                                <Select label="Ваш город" {...register('cityId', { required: true })}>
                                    <option value="">Выберите город</option>
                                    {cities.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                                </Select>

                                <Select
                                    label="Район"
                                    {...register('districtId')}
                                    disabled={!watchedCityId}
                                >
                                    <option value="">{isDistrictsLoading ? 'Загрузка...' : 'Не привязан'}</option>
                                    {districts.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                                </Select>
                            </div>

                            <div className={styles.addressSection}>
                                <Input
                                    label="Точный адрес (Улица, номер дома)"
                                    {...register('homeAddress')}
                                    placeholder="пр. Победителей, 1"
                                />

                                <div className={styles.warningCard}>
                                    <div className={styles.warningTitle}>⚠️ Требование ГИС-проверки</div>
                                    <p className={styles.warningText}>
                                        Указанный адрес <b>должен находиться внутри границ</b> выбранного вами района.
                                        Система проверит координаты: если адрес из другого района, профиль не обновится.
                                    </p>
                                </div>
                            </div>
                        </div>

                        <div className={styles.column}>
                            <label className={styles.uploaderLabel}>Фото профиля (JPG, PNG)</label>
                            <ImageUploader
                                multiple={false}
                                initialPreview={profile.avatarUrl}
                                onFilesChange={(f) => setAvatarFile(f[0])}
                            />
                        </div>
                    </div>

                    <div className={styles.editFooter}>
                        <Button variant="outline" type="button" onClick={() => setEditMode(false)}>Отменить</Button>
                        <Button type="submit" disabled={mutation.isPending}>
                            {mutation.isPending ? 'Синхронизация...' : 'Сохранить изменения'}
                        </Button>
                    </div>
                </form>
            )}
        </div>
    );
};