import React, { useEffect, useState } from 'react';
import { HubConnectionBuilder, HubConnection } from '@microsoft/signalr';

interface JobUpdate {
  jobId: string;
  jobType: string;
  jobName: string;
  status: string;
}

function App() {
  const [logs, setLogs] = useState<string[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [jobType, setJobType] = useState('');
  const [jobName, setJobName] = useState('');

  useEffect(() => {
    console.log('API_URL:', import.meta.env.VITE_API_URL);
    const connection: HubConnection = new HubConnectionBuilder()
      .withUrl('/jobHub')
      .withAutomaticReconnect()
      .build();

    connection.start()
      .then(() => {
        console.log('Connected to SignalR hub');
        connection.invoke("SubscribeToJobUpdates");
      })
      .catch(err => console.error('SignalR Connection Error:', err));

    connection.on("JobUpdated", (job: JobUpdate) => {
      const message = `[${new Date().toLocaleTimeString()}] Job ${job.jobId} (${job.jobType}) is ${job.status}`;
      setLogs(prev => [...prev, message]);
    });

    return () => {
      connection.stop();
    };
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const response = await fetch(import.meta.env.VITE_API_URL + '/jobs', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ jobType, jobName })
      });
      if (!response.ok) {
        const error = await response.json();
        let details = '';
        if (error.Details) {
          details = error.Details.join(', ');
        }
        console.log(details)
        alert(`Error: ${error.Message || details}`);
        return;
      }
      const result = await response.json();
      setLogs(prev => [...prev, `[${new Date().toLocaleTimeString()}] Created Job: ${result.jobId}`]);
      setJobType('');
      setJobName('');
      setShowForm(false);
    } catch (err) {
      alert("Failed to create job.");
      console.error(err);
    }
  }

  return (
    <div className="console-container">
      <h2 className="console-header">Job Console</h2>
      <div className="console-actions">
        <button onClick={() => setShowForm(!showForm)}>
          {showForm ? 'Close Form' : '➕ Create Job'}
        </button>
      </div>

      {showForm && (
        <form className="console-form" onSubmit={handleSubmit}>
          <input type="text" placeholder="Job Type" value={jobType} onChange={e => setJobType(e.target.value)} required />
          <input type="text" placeholder="Job Name" value={jobName} onChange={e => setJobName(e.target.value)} required />
          <button type="submit">Start Job</button>
        </form>
      )}

      <div className="console-output">
        {logs.map((log, idx) => (
          <div key={idx} className="console-line">{log}</div>
        ))}
      </div>
    </div>
  );
}

export default App;
