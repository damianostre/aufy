import {SubmitHandler, useForm} from "react-hook-form";
import {z} from "zod";
import {zodResolver} from "@hookform/resolvers/zod";
import {useState} from "react";
import {extractApiErrors} from "../../../lib/axios.ts";
import {auth} from "../../../api/auth.ts";

export const ChangePasswordForm = () => {
    const [apiErrors, setApiErrors] = useState<string[]>();
    const [notification, setNotification] = useState<string>();
    const {register, handleSubmit, formState: { isSubmitting, errors }} = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });
    const onSubmit: SubmitHandler<FormModel> = data => {
        return auth.changePassword({password: data.password, newPassword: data.newPassword}).then(() => {
            setNotification("Password changed successfully");
        }).catch((error) => {
            setApiErrors(extractApiErrors(error) ?? ["Error occured"]);
        });
    };

    return <form onSubmit={handleSubmit(onSubmit)} className="space-y-5" method="POST" noValidate={true}>

        {apiErrors && <div className="rounded-md bg-red-50 p-4">
            <div className="flex">
                <div className="ml-3">
                    <div className="mt-2 text-sm text-red-700">
                        <ul role="list" className="list-disc space-y-1 pl-5">
                            {apiErrors.map((error, index) => <li key={index}>{error}</li>)}
                        </ul>
                    </div>
                </div>
            </div>
        </div>}
        
        {notification && <div className="rounded-md bg-green-50 p-4">
            <div className="flex">
                <div className="ml-3">
                    <div className="mt-2 text-sm text-green-700">
                        <ul role="list" className="list-disc space-y-1 pl-5">
                            <li>{notification}</li>
                        </ul>
                    </div>
                </div>
            </div>
        </div>}

        <div>
            <label htmlFor="password" className="block text-sm font-medium leading-6 text-gray-900">
                Password
            </label>
            <div className="mt-2">
                <input
                    {...register("password", {required: true})}
                    type="password"
                    required
                    className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-inset focus:ring-black sm:text-sm sm:leading-6"
                />
            </div>
            <p className="mt-2 text-sm text-red-600" id="email-error">
                {errors.password?.message}
            </p>

        </div>

        <div>
            <label htmlFor="newPassword" className="block text-sm font-medium leading-6 text-gray-900">
                New Password
            </label>
            <div className="mt-2">
                <input
                    {...register("newPassword", {required: true})}
                    type="password"
                    required
                    className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-inset focus:ring-black sm:text-sm sm:leading-6"
                />
            </div>
            <p className="mt-2 text-sm text-red-600" id="email-error">
                {errors.newPassword?.message}
            </p>

        </div>

        <div>
            <label htmlFor="confirmNewPassword" className="block text-sm font-medium leading-6 text-gray-900">
                Confirm New Password
            </label>
            <div className="mt-2">
                <input
                    {...register("confirmNewPassword", {
                        required: true,
                    })}
                    type="password"
                    required
                    className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-inset focus:ring-black sm:text-sm sm:leading-6"
                />
            </div>
            <p className="mt-2 text-sm text-red-600" id="confirmPassword-error">
                {errors.confirmNewPassword?.message}
            </p>
        </div>

        <div>
            <button
                type="submit"
                className="flex w-full justify-center rounded-md hover:primary-bg primary-bg px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                disabled={isSubmitting}
            >
                {isSubmitting ? "Processing..." : "Change"}
            </button>
        </div>
    </form>
};

const validationSchema = z.object({
    password: z
        .string()
        .min(6, {message: "Password must be at least 6 characters"}),
    newPassword: z
        .string()
        .min(6, {message: "Password must be at least 6 characters"}),
    confirmNewPassword: z
        .string()
        .min(1, {message: "Confirm Password is required"}),
})
    .refine((data) => data.newPassword === data.confirmNewPassword, {
        path: ["confirmPassword"],
        message: "Password don't match",
    });
type FormModel = z.infer<typeof validationSchema>;