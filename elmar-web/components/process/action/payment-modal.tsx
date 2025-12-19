'use client';

import { useState } from 'react';
import { Dialog } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { customInstance } from '@/lib/api/custom-instance';

interface PaymentModalProps {
    isOpen: boolean;
    onClose: () => void;
    amount: number;
    onSuccess: (txId: string) => void;
}

export function PaymentModal({ isOpen, onClose, amount, onSuccess }: PaymentModalProps) {
    const [cardToken, setCardToken] = useState('4111111111111111');
    const [loading, setLoading] = useState(false);

    const handlePay = async () => {
        setLoading(true);
        try {
            const res = await customInstance<{ transactionId: string }>({
                url: '/api/payment/process',
                method: 'POST',
                data: { amount, cardToken }
            });
            onSuccess(res.transactionId);
        } catch (e) {
            alert("Payment Failed");
        } finally {
            setLoading(false);
        }
    };

    return (
        <Dialog isOpen={isOpen} onClose={onClose} title={`Payment Required: ${amount} TRY`}>
            <div className="space-y-4">
                <div>
                    <Label>Card Number (Mock)</Label>
                    <Input value={cardToken} onChange={e => setCardToken(e.target.value)} />
                    <p className="text-xs text-gray-500">Use '4111...' for success, 'FAIL' for error.</p>
                </div>
                <Button onClick={handlePay} disabled={loading} className="w-full">
                    {loading ? 'Processing...' : `Pay ${amount} TRY`}
                </Button>
            </div>
        </Dialog>
    );
}
