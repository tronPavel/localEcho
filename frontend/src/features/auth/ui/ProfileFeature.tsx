import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { Button } from '@/shared/ui/Button/Button';
import { Input } from '@/shared/ui/Input/Input';
import { Select } from '@/shared/ui/Select/Select';
import { UserAvatar } from '@/entities/user/ui/UserAvatar';
import { getMyProfile, updateProfile } from '@/entities/user/model/userApi';
import { getDistricts } from '@/entities/district/model/districtApi';
import { useAuthStore } from '@/features/auth/model/authStore';
import { logout } from '@/features/auth/model/authApi';
import type { UpdateProfileDto } from '@/entities/user/model/types';
import styles from './ProfileFeature.module.css';

export const ProfileFeature = ({ onActionSuccess }: { onActionSuccess: () => void }) => {
    const { user, clearUser } = useAuthStore();
    const queryClient = useQueryClient();
    const [editMode, setEditMode] = useState(false);
    const [avatarFile, setAvatarFile] = useState<File | undefined>();
    const [previewUrl, setPreviewUrl] = useState<string | null>(null);

    const { data: profile, refetch } = useQuery({ queryKey: ['profile'], queryFn: getMyProfile });
    const { data: districts = [] } = useQuery({ queryKey: ['districts'], queryFn: getDistricts });

    const { register, handleSubmit, setValue } = useForm<UpdateProfileDto>();

    useEffect(() => {
        if (profile && editMode) {
            setValue('name', profile.name);
            setValue('homeAddress', profile.homeAddress);
            setValue('districtId', profile.district?.id);
        }
    }, [profile, editMode, setValue]);

    const mutation = useMutation({
        mutationFn: updateProfile,
        onSuccess: () => {
            setEditMode(false);
            queryClient.invalidateQueries({ queryKey: ['profile'] });
            refetch();
        },
    });

    const handleLogout = async () => {
        await logout();
        clearUser();
        onActionSuccess();
    };

    if (!user || !profile) return <div>Загрузка профиля...</div>;

    return (
        <div className={styles.profile}>
            <UserAvatar user={{ name: profile.name, avatarUrl: previewUrl || profile.avatarUrl }} size="large" />

            {editMode ? (
                <form onSubmit={handleSubmit((d) => mutation.mutate({ ...d, avatarFile }))} className={styles.formStack}>
                    <Input placeholder="Имя" {...register('name')} />
                    <Input placeholder="Адрес" {...register('homeAddress')} />
                    <Select {...register('districtId')}>
                        {districts.map(d => <option key={d.id} value={d.id}>{d.name}</option>)}
                    </Select>
                    <Input type="file" accept="image/*" onChange={(e) => {
                        const file = e.target.files?.[0];
                        if (file) { setAvatarFile(file); setPreviewUrl(URL.createObjectURL(file)); }
                    }} />
                    <div className={styles.buttons}>
                        <Button type="submit">Сохранить</Button>
                        <Button variant="secondary" onClick={() => setEditMode(false)}>Отмена</Button>
                    </div>
                </form>
            ) : (
                <>
                    <h2>{profile.name}</h2>
                    <p>Баллы: {profile.points}</p>
                    <p>Район: {profile.district?.name || 'Не указан'}</p>
                    <div className={styles.buttons}>
                        <Button onClick={() => setEditMode(true)}>Редактировать</Button>
                        <Button variant="outline" onClick={handleLogout}>Выйти</Button>
                    </div>
                </>
            )}
        </div>
    );
};