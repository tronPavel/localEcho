import { useMutation, useQueryClient } from '@tanstack/react-query';
import { vote } from '../model/voteApi';
import styles from './VoteButtons.module.css';
import {Button} from "../../../shared/ui/Button/Button.tsx";
import {Icon} from "../../../shared/ui/Icon/Icon.tsx";

interface VoteButtonsProps {
    markerId: string;
    currentVote: number;
    rating: number;
}

export const VoteButtons = ({ markerId, currentVote, rating }: VoteButtonsProps) => {
    const queryClient = useQueryClient();
    const mutation = useMutation({
        mutationFn: (isUpvote: boolean) => vote(markerId, isUpvote),
        onSuccess: () => {
            queryClient.invalidateQueries({ queryKey: ['marker', markerId] });
            queryClient.invalidateQueries({ queryKey: ['markers'] });
        },
    });

    return (
        <div className={styles.buttons}>
            <Button
                variant="outline"
                onClick={() => mutation.mutate(true)}
                className={currentVote === 1 ? styles.active : ''}
            >
                <Icon type="up" /> {rating > 0 ? `+${rating}` : rating}
            </Button>
            <Button
                variant="outline"
                onClick={() => mutation.mutate(false)}
                className={currentVote === -1 ? styles.active : ''}
            >
                <Icon type="down" />
            </Button>
        </div>
    );
};