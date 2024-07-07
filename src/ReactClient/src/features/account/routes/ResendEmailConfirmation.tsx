import {useState} from "react";
import {SubmitHandler, useForm} from "react-hook-form";
import {zodResolver} from "@hookform/resolvers/zod";
import {auth} from "../../../api/auth.ts";
import {extractApiErrors} from "../../../lib/axios.ts";
import {z} from "zod";

export const ResendEmailConfirmation = () => {
    const [apiErrors, setApiErrors] = useState<string[] | null>();
    const [notification, setNotification] = useState<string>();

    const {
        register,
        handleSubmit,
        formState: {errors, isSubmitting, isSubmitSuccessful},
        reset
    } = useForm<FormModel>({
        resolver: zodResolver(validationSchema),
    });
    const onSubmit: SubmitHandler<FormModel> = data => {
        return auth.resendEmailConfirmation(data.email).then(() => {
            setApiErrors(null);
            setNotification("Email sent successfully");
            reset();
        }).catch((error) => {
            setApiErrors(extractApiErrors(error) ?? ["Error occured"]);
        });
    };

    return <>
        <h1>Don't get the email?</h1>
        <div className="mt-4 max-w-md">
            <form onSubmit={handleSubmit(onSubmit)} className="space-y-5" method="POST" noValidate={true}>

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

                <div>
                    <label htmlFor="email" className="block text-sm font-medium leading-6 text-gray-900">
                        Email address
                    </label>
                    <div className="mt-2">
                        <input
                            {...register("email")}
                            type="email"
                            autoComplete="email"
                            className="block w-full rounded-md border-0 py-1.5 text-gray-900 shadow-sm ring-1 ring-inset ring-gray-300 placeholder:text-gray-400 focus:ring-inset focus:ring-black sm:text-sm sm:leading-6"
                        />
                    </div>
                    <p className="mt-2 text-sm text-red-600" id="email-error">
                        {errors.email?.message}
                    </p>
                </div>

                <div>
                    <button
                        type="submit"
                        className="flex w-full justify-center rounded-md hover:primary-bg primary-bg px-3 py-1.5 text-sm font-semibold leading-6 text-white shadow-sm focus-visible:outline focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                        disabled={isSubmitting || isSubmitSuccessful}
                    >
                        {isSubmitting ? "Sending..." :
                            isSubmitSuccessful ? "Email Sent" :
                                "Resend Confirmation Email"}
                    </button>
                </div>
            </form>
        </div>
    </>
};

const validationSchema = z.object({
    email: z
        .string().min(1, {message: "Email is required"})
        .email({message: "Must be a valid email",})
});
type FormModel = z.infer<typeof validationSchema>;
