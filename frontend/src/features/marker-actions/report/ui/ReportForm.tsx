import { useForm } from 'react-hook-form';
import { useReport } from '../model/useReport';
import styles from './ReportForm.module.css';
import {Button} from "@/shared/ui/Button/Button.tsx";
import {Select} from "@/shared/ui/Select/Select.tsx";
import {Textarea} from "@/shared/ui/Textarea/Textarea.tsx";
import  {ReportReason} from "@/features/marker-actions/report/api/reportApi.ts";

interface FormFields {
    reason: string;
    comment: string;
}

export const ReportForm = ({ markerId, onSuccess }: { markerId: string, onSuccess: () => void }) => {
    const { mutate, isPending } = useReport(markerId);

    const { register, handleSubmit } = useForm<FormFields>();

    const onSubmit = (data: FormFields) => {
        mutate({
            reason: Number(data.reason) as ReportReason,
            comment: data.comment
        }, {
            onSuccess: () => onSuccess()
        });
    };

    return (
        <form onSubmit={handleSubmit(onSubmit)} className={styles.form}>
            <p className={styles.info}>
                Пожалуйста, укажите причину, по которой данная метка должна быть проверена модератором.
            </p>

            <Select label="Что не так с этой меткой?" {...register('reason')}>
                <option value={ReportReason.Spam}>Это спам / реклама</option>
                <option value={ReportReason.Offense}>Оскорбления или мат</option>
                <option value={ReportReason.Inaccurate}>Неверная категория или статус</option>
                <option value={ReportReason.Fake}>Этого события/проблемы не существует</option>
                <option value={ReportReason.Other}>Другая причина</option>
            </Select>

            <Textarea
                label="Комментарий (необязательно)"
                placeholder="Расскажите подробнее..."
                {...register('comment')}
            />

            <div className={styles.actions}>
                <Button type="submit" disabled={isPending}>

                    {isPending ? 'Отправка...' : '⚠️ Отправить жалобу'}
                </Button>
            </div>
        </form>
    );
};