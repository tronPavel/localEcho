import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { Button } from '@/shared/ui/Button/Button';
import { Textarea } from '@/shared/ui/Textarea/Textarea';
import { changeMarkerStatus } from '../model/viewMarkerApi';
import styles from './MarkerResolveFeature.module.css';
import {ImageUploader} from "@/shared/ui/ImageUploader/ImageUploader.tsx";

interface MarkerResolveFeatureProps {
    markerId: string;
}

export const MarkerResolveFeature = ({ markerId }: MarkerResolveFeatureProps) => {
    const queryClient = useQueryClient();
    const navigate = useNavigate();
    const [files, setFiles] = useState<File[]>([]);

    const { register, handleSubmit, formState: { errors } } = useForm({
        defaultValues: { comment: '' }
    });

    const mutation = useMutation({
        mutationFn: (data: { comment: string }) => changeMarkerStatus({
            markerId,
            newStatus: 'Resolved', // Фиксируем статус на "Решено"
            comment: data.comment,
            imageFiles: files
        }),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['marker', markerId] });
            queryClient.invalidateQueries({ queryKey: ['markers'] });
            navigate(`/marker/${markerId}`); // Возвращаемся к просмотру метки
        }
    });

    return (
        <form onSubmit={handleSubmit(d => mutation.mutate(d))} className={styles.form}>
            <div className={styles.alert}>
                Вы действуете как официальное лицо. Ваш ответ будет закреплен в истории метки.
            </div>

            <Textarea
                label="Что было сделано?"
                placeholder="Опишите результат работ детально..."
                {...register('comment', { required: 'Опишите решение' })}
                error={errors.comment?.message}
            />

            <div className={styles.dropzone}>
                <ImageUploader onFilesChange={(f) => setFiles(f)} />
            </div>

            <div className={styles.footer}>
                <Button type="submit" disabled={mutation.isPending}>
                    ✅ Подтвердить и закрыть проблему
                </Button>
                <Button variant="outline" type="button" onClick={() => navigate(-1)}>
                    Отмена
                </Button>
            </div>
        </form>
    );
};